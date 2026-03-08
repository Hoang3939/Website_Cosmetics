"""
Clean RAG demo script for Website_Cosmetics.

- No hard-coded API keys or DB passwords.
- Uses environment variables.
- Supports embedding generation + in-memory retrieval demo.
"""

from __future__ import annotations

import ast
import json
import os
import time
from collections import deque
from dataclasses import dataclass
from typing import Any

import numpy as np
import psycopg2
from google import genai
from google.genai import types


@dataclass
class PgConfig:
    host: str
    dbname: str
    user: str
    password: str
    port: int


@dataclass
class GeminiConfig:
    embed_api_key: str
    decision_api_key: str
    chat_api_key: str
    embed_model: str = "gemini-embedding-001"
    decision_model: str = "gemini-2.5-flash"
    chat_model: str = "gemini-2.5-flash"


def load_pg_config() -> PgConfig:
    return PgConfig(
        host=os.getenv("RAG_PG_HOST", "localhost"),
        dbname=os.getenv("RAG_PG_DB", "rag_demo"),
        user=os.getenv("RAG_PG_USER", "postgres"),
        password=os.getenv("RAG_PG_PASSWORD", ""),
        port=int(os.getenv("RAG_PG_PORT", "5432")),
    )


def load_gemini_config() -> GeminiConfig:
    embed_key = os.getenv("GEMINI_EMBED_API_KEY", "")
    decision_key = os.getenv("GEMINI_DECISION_API_KEY", "")
    chat_key = os.getenv("GEMINI_CHAT_API_KEY", "")

    if not embed_key or not decision_key or not chat_key:
        raise ValueError(
            "Missing Gemini API keys. Set GEMINI_EMBED_API_KEY, "
            "GEMINI_DECISION_API_KEY, GEMINI_CHAT_API_KEY."
        )

    return GeminiConfig(
        embed_api_key=embed_key,
        decision_api_key=decision_key,
        chat_api_key=chat_key,
    )


def connect_pg(cfg: PgConfig):
    conn = psycopg2.connect(
        host=cfg.host,
        dbname=cfg.dbname,
        user=cfg.user,
        password=cfg.password,
        port=cfg.port,
    )
    conn.autocommit = False
    return conn


def load_products(conn) -> list[tuple[Any, str, Any, Any]]:
    with conn.cursor() as cur:
        cur.execute(
            """
            SELECT productid, description, embedding, price
            FROM product
            WHERE description IS NOT NULL
            """
        )
        return cur.fetchall()


def create_embeddings_for_missing(conn, embed_client: genai.Client, model: str) -> int:
    with conn.cursor() as cur:
        cur.execute(
            """
            SELECT productid, description
            FROM product
            WHERE description IS NOT NULL
              AND (embedding IS NULL OR embedding = '')
            """
        )
        rows = cur.fetchall()

    if not rows:
        return 0

    descriptions = [r[1] for r in rows]
    result = embed_client.models.embed_content(
        model=model,
        contents=descriptions,
        config=types.EmbedContentConfig(task_type="RETRIEVAL_DOCUMENT"),
    )

    embeddings = [e.values for e in result.embeddings]

    with conn.cursor() as cur:
        for i, (product_id, _) in enumerate(rows):
            cur.execute(
                "UPDATE product SET embedding = %s WHERE productid = %s",
                (json.dumps(embeddings[i]), product_id),
            )

    conn.commit()
    return len(rows)


def parse_embedding(raw: Any) -> np.ndarray | None:
    if raw is None:
        return None

    if isinstance(raw, list):
        return np.array(raw, dtype=float)

    if isinstance(raw, str):
        try:
            return np.array(ast.literal_eval(raw), dtype=float)
        except Exception:
            return None

    return None


def cosine_similarity(query: np.ndarray, matrix: np.ndarray) -> np.ndarray:
    query_norm = np.linalg.norm(query)
    matrix_norm = np.linalg.norm(matrix, axis=1)
    denom = np.maximum(query_norm * matrix_norm, 1e-10)
    return np.dot(matrix, query) / denom


def decide_action(
    client_decision: genai.Client,
    model: str,
    user_message: str,
    history: deque[tuple[str, str]],
) -> dict[str, str]:
    summary = "\n".join(
        [f"{i+1}. User: {u}\n   Bot: {b}" for i, (u, b) in enumerate(history)]
    ) or "No previous conversation."

    prompt = f"""
You are a decision-making assistant for a website chatbot.

Your goal: decide whether the chatbot should use RAG (retrieve data) or CHAT (casual conversation).

Output format:
{{
  "action": "rag" | "chat",
  "reason": "short explanation"
}}

Decision rules:
- Choose "rag" when user asks product/data/filtering information.
- Choose "chat" for greetings, reactions, thanks, and casual continuation.
- If unsure, default to "chat".

Conversation Summary:
{summary}

User Message:
{user_message}

Return only valid JSON.
"""

    response = client_decision.models.generate_content(
        model=model,
        contents=prompt,
        config=types.GenerateContentConfig(
            temperature=0,
            response_mime_type="application/json",
        ),
    )

    try:
        return json.loads(response.text)
    except Exception:
        return {"action": "chat", "reason": "fallback: invalid JSON"}


def chat_response(client_chat: genai.Client, model: str, message: str) -> str:
    system_context = (
        "You are a friendly shopping website chatbot. "
        "Reply naturally and concisely in 1-2 sentences."
    )
    prompt = f"{system_context}\nUser: {message}\nChatbot:"

    resp = client_chat.models.generate_content(
        model=model,
        contents=prompt,
        config=types.GenerateContentConfig(temperature=0.7),
    )
    return resp.text


def main() -> None:
    pg_cfg = load_pg_config()
    gemini_cfg = load_gemini_config()

    embed_client = genai.Client(api_key=gemini_cfg.embed_api_key)
    decision_client = genai.Client(api_key=gemini_cfg.decision_api_key)
    chat_client = genai.Client(api_key=gemini_cfg.chat_api_key)

    conn = connect_pg(pg_cfg)
    try:
        updated = create_embeddings_for_missing(conn, embed_client, gemini_cfg.embed_model)
        print(f"Embeddings created for {updated} products.")

        data = load_products(conn)
        vectors, names, prices = [], [], []
        for _, desc, emb, price in data:
            vec = parse_embedding(emb)
            if vec is not None:
                vectors.append(vec)
                names.append(desc)
                prices.append(price)

        if not vectors:
            print("No vectors found in database. Please generate embeddings first.")
            return

        matrix = np.array(vectors, dtype=float)
        print(f"Loaded {len(matrix)} vectors.")

        history: deque[tuple[str, str]] = deque(maxlen=5)

        while True:
            user_message = input("User: ").strip()
            if user_message.lower() in {"quit", "exit"}:
                print("Exiting.")
                break

            decision = decide_action(
                decision_client,
                gemini_cfg.decision_model,
                user_message,
                history,
            )

            if decision.get("action") == "rag":
                q = embed_client.models.embed_content(
                    model=gemini_cfg.embed_model,
                    contents=user_message,
                    config=types.EmbedContentConfig(task_type="RETRIEVAL_QUERY"),
                ).embeddings[0].values

                sims = cosine_similarity(np.array(q, dtype=float), matrix)
                top_idx = np.argsort(sims)[::-1][:5]
                retrieved_text = "\n".join(
                    [f"- {names[i]} | price: {prices[i]}" for i in top_idx]
                )

                final_prompt = (
                    f"User asked: {user_message}\n"
                    f"Relevant products:\n{retrieved_text}\n"
                    "Answer naturally."
                )
            else:
                final_prompt = user_message

            answer = chat_response(chat_client, gemini_cfg.chat_model, final_prompt)
            print(f"Bot: {answer}\n")
            history.append((user_message, answer))
            time.sleep(0.2)
    finally:
        conn.close()


if __name__ == "__main__":
    main()
