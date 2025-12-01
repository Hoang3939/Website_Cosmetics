"""
Script to generate embeddings for all products in the database.
This script connects to SQL Server, retrieves products with descriptions,
generates embeddings using Google Gemini API, and saves them back to the database.
"""

import pyodbc
import json
import time
import sys
from google import genai
from google.genai import types

# Set UTF-8 encoding for Windows console
if sys.platform == 'win32':
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

# Configuration
DB_CONFIG = {
    "server": "34.142.130.206,1433",
    "database": "WebsiteCosmetic",
    "user": "sa",
    "password": "23DPM@NHHTeam",
    "driver": "{ODBC Driver 17 for SQL Server}"
}

# Gemini API Key
GEMINI_API_KEY = "AIzaSyAuqsBJCUT8C55wNyHj4jn6nMn4c1y1kdM"

def get_connection_string():
    """Build connection string for SQL Server"""
    return (
        f"DRIVER={DB_CONFIG['driver']};"
        f"SERVER={DB_CONFIG['server']};"
        f"DATABASE={DB_CONFIG['database']};"
        f"UID={DB_CONFIG['user']};"
        f"PWD={DB_CONFIG['password']};"
        "TrustServerCertificate=yes;"
    )

def get_products_without_embeddings(conn):
    """Get all products that need embeddings"""
    cursor = conn.cursor()
    query = """
        SELECT ProductId, Description 
        FROM Product 
        WHERE Description IS NOT NULL 
        AND Description != '' 
        AND (Embedding IS NULL OR Embedding = '')
    """
    cursor.execute(query)
    return cursor.fetchall()

def generate_embedding(text, client, max_retries=3):
    """Generate embedding for a single text using Gemini API with retry logic"""
    for attempt in range(max_retries):
        try:
            result = client.models.embed_content(
                model="text-embedding-004",
                contents=text,
                config=types.EmbedContentConfig(task_type="RETRIEVAL_DOCUMENT")
            )
            if result.embeddings and len(result.embeddings) > 0:
                return result.embeddings[0].values
            return None
        except Exception as e:
            error_str = str(e)
            if "429" in error_str or "RESOURCE_EXHAUSTED" in error_str:
                if attempt < max_retries - 1:
                    wait_time = (2 ** attempt) * 5  # Exponential backoff: 5s, 10s, 20s
                    print(f"   Rate limit exceeded. Waiting {wait_time}s before retry {attempt + 1}/{max_retries}...")
                    time.sleep(wait_time)
                    continue
                else:
                    print(f"Error generating embedding: Rate limit exceeded after {max_retries} retries")
                    print("   Please wait a few minutes and try again, or check your API quota")
                    return None
            else:
                print(f"Error generating embedding: {e}")
                return None
    return None

def save_embedding(conn, product_id, embedding):
    """Save embedding to database as JSON string"""
    cursor = conn.cursor()
    embedding_json = json.dumps(embedding)
    query = "UPDATE Product SET Embedding = ? WHERE ProductId = ?"
    try:
        cursor.execute(query, (embedding_json, product_id))
        conn.commit()
        return True
    except Exception as e:
        print(f"Error saving embedding for product {product_id}: {e}")
        conn.rollback()
        return False

def main():
    print("Starting embedding generation...")
    
    # Initialize Gemini client
    if GEMINI_API_KEY == "YOUR_GEMINI_API_KEY_HERE":
        print("ERROR: Please set your GEMINI_API_KEY in the script")
        return
    
    client = genai.Client(api_key=GEMINI_API_KEY)
    
    # Connect to database
    try:
        conn_str = get_connection_string()
        conn = pyodbc.connect(conn_str)
        print("Connected to SQL Server")
    except Exception as e:
        print(f"ERROR connecting to database: {e}")
        return
    
    try:
        # Get products
        products = get_products_without_embeddings(conn)
        print(f"Found {len(products)} products to process")
        
        if len(products) == 0:
            print("All products already have embeddings")
            return
        
        # Process each product
        success_count = 0
        error_count = 0
        
        for i, (product_id, description) in enumerate(products, 1):
            print(f"\n[{i}/{len(products)}] Processing Product ID: {product_id}")
            try:
                desc_preview = description[:100] if description else "No description"
                print(f"   Description: {desc_preview}...")
            except UnicodeEncodeError:
                print(f"   Description: (contains special characters)")
            
            # Generate embedding
            embedding = generate_embedding(description, client)
            
            if embedding:
                # Save to database
                if save_embedding(conn, product_id, embedding):
                    success_count += 1
                    print(f"   [OK] Embedding saved successfully")
                else:
                    error_count += 1
                    print(f"   [ERROR] Failed to save embedding")
            else:
                error_count += 1
                print(f"   [ERROR] Failed to generate embedding")
            
            # Rate limiting - wait longer between requests to avoid quota issues
            if i < len(products):
                time.sleep(1)  # Increased to 1 second to avoid rate limits
        
        print(f"\n{'='*50}")
        print(f"Completed!")
        print(f"   Success: {success_count}")
        print(f"   Errors: {error_count}")
        print(f"{'='*50}")
        
    except Exception as e:
        print(f"ERROR: {e}")
    finally:
        conn.close()
        print("\nDatabase connection closed")

if __name__ == "__main__":
    main()

