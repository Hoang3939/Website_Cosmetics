"""
Clean RAG demo pipeline for PostgreSQL + Gemini embeddings.

This script is converted from `rag_demo.ipynb` and removes all hard-coded secrets.
Configuration is loaded from environment variables.

Required environment variables:
- GEMINI_API_KEY
- PGHOST
- PGDATABASE
- PGUSER
- PGPASSWORD

Optional environment variables:
- PGPORT (default: 5432)
- EMBEDDING_MODEL (default: gemini-embedding-001)
- EMBEDDING_TASK_TYPE (default: RETRIEVAL_DOCUMENT)
"""

from __future__ import annotations

import argparse
import json
import os
import sys
from typing import List, Sequence, Tuple

import psycopg2
from google import genai
from google.genai import types


def get_env(name: str, default: str | None = None, required: bool = False) -> str:
    value = os.getenv(name, default)
    if required and (value is None or value == ""):
        raise ValueError(f"Missing required environment variable: {name}")
    return value or ""


def create_db_connection():
    return psycopg2.connect(
        host=get_env("PGHOST", required=True),
        dbname=get_env("PGDATABASE", required=True),
        user=get_env("PGUSER", required=True),
        password=get_env("PGPASSWORD", required=True),
        port=int(get_env("PGPORT", "5432")),
    )


def fetch_products(cursor, only_missing: bool) -> List[Tuple[str, str]]:
    if only_missing:
        cursor.execute(
            """
            SELECT productid, description
            FROM product
            WHERE description IS NOT NULL
              AND TRIM(description) <> ''
              AND (embedding IS NULL OR TRIM(CAST(embedding AS TEXT)) = '')
            ORDER BY productid
            """
        )
    else:
        cursor.execute(
            """
            SELECT productid, description
            FROM product
            WHERE description IS NOT NULL
              AND TRIM(description) <> ''
            ORDER BY productid
            """
        )

    return [(str(row[0]), row[1]) for row in cursor.fetchall()]


def create_embeddings(client: genai.Client, texts: Sequence[str]) -> List[List[float]]:
    model = get_env("EMBEDDING_MODEL", "gemini-embedding-001")
    task_type = get_env("EMBEDDING_TASK_TYPE", "RETRIEVAL_DOCUMENT")

    result = client.models.embed_content(
        model=model,
        contents=list(texts),
        config=types.EmbedContentConfig(task_type=task_type),
    )

    return [embedding.values for embedding in result.embeddings]


def update_embeddings(cursor, rows: Sequence[Tuple[str, str]], embeddings: Sequence[Sequence[float]]) -> int:
    updated = 0
    for (product_id, _), vector in zip(rows, embeddings):
        embedding_json = json.dumps(vector)
        cursor.execute(
            "UPDATE product SET embedding = %s WHERE productid = %s",
            (embedding_json, product_id),
        )
        updated += 1
    return updated


def run(only_missing: bool, dry_run: bool) -> int:
    api_key = get_env("GEMINI_API_KEY", required=True)
    client = genai.Client(api_key=api_key)

    with create_db_connection() as conn:
        with conn.cursor() as cursor:
            rows = fetch_products(cursor, only_missing=only_missing)
            if not rows:
                print("No products to process.")
                return 0

            descriptions = [description for _, description in rows]
            print(f"Loaded {len(rows)} products from database.")

            embeddings = create_embeddings(client, descriptions)
            print(f"Created {len(embeddings)} embeddings.")

            if dry_run:
                print("Dry run enabled: no database updates were made.")
                return 0

            updated = update_embeddings(cursor, rows, embeddings)
            conn.commit()
            print(f"Saved embeddings for {updated} products.")

    return 0


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate and store product embeddings for RAG demo")
    parser.add_argument(
        "--all",
        action="store_true",
        help="Process all products with descriptions (default: only rows missing embedding)",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Run embedding generation without updating database",
    )
    return parser.parse_args()


def main() -> None:
    try:
        args = parse_args()
        only_missing = not args.all
        raise SystemExit(run(only_missing=only_missing, dry_run=args.dry_run))
    except ValueError as exc:
        print(f"Configuration error: {exc}")
        raise SystemExit(2) from exc
    except Exception as exc:
        print(f"Unexpected error: {exc}")
        raise SystemExit(1) from exc


if __name__ == "__main__":
    main()
