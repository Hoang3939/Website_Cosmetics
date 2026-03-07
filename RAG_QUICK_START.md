# RAG Quick Start Guide

## Tóm tắt

Hệ thống RAG đã được tích hợp vào Website_Cosmetics. Chatbot có thể:
- Tự động quyết định khi nào cần tìm kiếm sản phẩm (RAG) hay chỉ trò chuyện (Chat)
- Tìm kiếm sản phẩm tương tự dựa trên câu hỏi của người dùng
- Trả lời tự nhiên bằng tiếng Việt

## Các bước setup nhanh

### 1. Cấu hình API Keys

Mở `appsettings.json` và thêm API keys của bạn:

```json
"Gemini": {
  "EmbedApiKey": "YOUR_KEY_HERE",
  "DecisionApiKey": "YOUR_KEY_HERE", 
  "ChatApiKey": "YOUR_KEY_HERE"
}
```

**Lưu ý:** Bạn có thể dùng 1 key cho cả 3, nhưng tách ra để dễ quản lý.

### 2. Chạy Migration

Chạy SQL script:
```sql
-- File: Database/AddEmbeddingToProduct.sql
```

Hoặc chạy trực tiếp:
```sql
ALTER TABLE Product ADD Embedding NVARCHAR(MAX) NULL;
```

### 3. Generate Embeddings

**Cách 1: Dùng Python script (Khuyến nghị)**

```bash
cd scripts
pip install -r requirements.txt
# Sửa GEMINI_API_KEY trong generate_embeddings.py
python generate_embeddings.py
```

**Cách 2: Dùng API endpoint**

```bash
POST /api/chat/generate-embeddings
```

### 4. Test API

```bash
POST /api/chat
Content-Type: application/json

{
  "message": "son môi màu đỏ giá dưới 500k",
  "sessionId": "optional"
}
```

## Cấu trúc Files

```
Website_Cosmetics/
├── RAG_SETUP_GUIDE.md          # Hướng dẫn chi tiết
├── RAG_QUICK_START.md          # File này
├── scripts/
│   ├── generate_embeddings.py  # Script Python
│   └── requirements.txt        # Python dependencies
├── Services/
│   ├── IRAGService.cs          # Interface
│   └── RAGService.cs           # Implementation
├── Controllers/
│   └── ChatController.cs       # API endpoints
├── Models/
│   └── Product.cs              # Updated với Embedding
└── Database/
    └── AddEmbeddingToProduct.sql
```

## API Endpoints

### POST /api/chat
Chat với bot

**Request:**
```json
{
  "message": "son môi màu đỏ",
  "sessionId": "session-123"
}
```

**Response:**
```json
{
  "response": "Chào bạn! Son môi màu đỏ có các sản phẩm...",
  "action": "rag",
  "reason": "User asks for products",
  "products": [
    {
      "productId": 1,
      "name": "Son môi 3CE...",
      "price": 450000,
      "similarity": 0.85,
      "description": "..."
    }
  ]
}
```

### POST /api/chat/generate-embeddings
Generate embeddings (cần authentication)

**Query params:**
- `productId` (optional): Generate cho 1 product cụ thể
- Không có `productId`: Generate cho tất cả products

## Troubleshooting

**Lỗi: "Gemini API key not configured"**
→ Kiểm tra `appsettings.json` có đúng key không

**Lỗi: "No products found"**
→ Chạy script generate embeddings

**Lỗi: "Cannot connect to database"**
→ Kiểm tra connection string trong `appsettings.json`

## Next Steps

1. ✅ Setup hoàn tất - Test API
2. Thêm UI chat widget vào frontend
3. Thêm conversation history vào database
4. Tối ưu performance với caching


