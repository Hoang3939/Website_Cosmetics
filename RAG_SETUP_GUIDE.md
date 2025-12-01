# Hướng dẫn Setup RAG (Retrieval-Augmented Generation) cho Website_Cosmetics

## Tổng quan

Hệ thống RAG này sử dụng Google Gemini API để:
- Tạo embeddings cho product descriptions
- Tìm kiếm sản phẩm tương tự dựa trên câu hỏi của người dùng
- Quyết định tự động khi nào dùng RAG (tìm kiếm) hay CHAT (trò chuyện thông thường)
- Trả lời câu hỏi về sản phẩm một cách tự nhiên

## Kiến trúc

```
User Query → Decision Model → RAG hoặc Chat → Gemini → Response
                ↓
         [Embedding Search] → Top Products → Context → Gemini
```

## Bước 1: Cài đặt Dependencies

### 1.1. Python Dependencies (cho script generate embeddings)

```bash
pip install google-genai psycopg2-binary numpy scikit-learn
```

### 1.2. C# NuGet Packages

Thêm vào `Website_Cosmetics.csproj`:

```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

Sau đó chạy:
```bash
dotnet restore
```

## Bước 2: Cấu hình API Keys

Thêm vào `appsettings.json`:

```json
{
  "Gemini": {
    "EmbedApiKey": "YOUR_EMBED_API_KEY",
    "DecisionApiKey": "YOUR_DECISION_API_KEY",
    "ChatApiKey": "YOUR_CHAT_API_KEY"
  }
}
```

**Lưu ý:** 
- Bạn cần 3 API keys riêng biệt từ Google AI Studio (https://aistudio.google.com/)
- Hoặc có thể dùng 1 key cho cả 3, nhưng tách ra để dễ quản lý rate limits

## Bước 3: Database Migration

### 3.1. Thêm Embedding column vào Product table

Chạy migration đã tạo:
```bash
dotnet ef migrations add AddEmbeddingToProduct
dotnet ef database update
```

Hoặc chạy SQL trực tiếp:
```sql
ALTER TABLE Product ADD Embedding NVARCHAR(MAX) NULL;
```

## Bước 4: Generate Embeddings cho Products hiện có

### 4.1. Chạy script Python

```bash
cd Website_Cosmetics
python scripts/generate_embeddings.py
```

Script này sẽ:
- Kết nối đến SQL Server database
- Lấy tất cả products có Description
- Tạo embeddings bằng Gemini API
- Lưu embeddings vào database dưới dạng JSON string

## Bước 5: Đăng ký Services

Đã được thêm vào `Program.cs`:
```csharp
builder.Services.AddScoped<IRAGService, RAGService>();
```

## Bước 6: Sử dụng API

### 6.1. Endpoint Chat

**POST** `/api/chat`

Request:
```json
{
  "message": "son môi màu đỏ giá dưới 500k",
  "sessionId": "optional-session-id"
}
```

Response:
```json
{
  "response": "Chào bạn! Son môi màu đỏ dưới 500k có các sản phẩm sau...",
  "action": "rag",
  "products": [
    {
      "productId": 1,
      "name": "Son môi 3CE...",
      "price": 450000,
      "similarity": 0.85
    }
  ]
}
```

### 6.2. Endpoint Generate Embeddings (Admin)

**POST** `/api/chat/generate-embeddings`

Chỉ dành cho admin, để generate embeddings cho products mới hoặc cập nhật.

## Cấu trúc Code

```
Website_Cosmetics/
├── Services/
│   └── RAGService.cs          # Core RAG logic
├── Controllers/
│   └── ChatController.cs      # API endpoints
├── Models/
│   └── Product.cs             # Updated với Embedding property
├── scripts/
│   └── generate_embeddings.py # Python script để generate embeddings
└── Migrations/
    └── AddEmbeddingToProduct.cs
```

## Flow hoạt động

1. **User gửi message** → ChatController
2. **Decision Model** (Gemini) quyết định:
   - `rag`: Nếu cần tìm kiếm sản phẩm
   - `chat`: Nếu chỉ trò chuyện thông thường
3. **Nếu RAG:**
   - Tạo embedding cho query
   - Tìm top 5 products tương tự (cosine similarity)
   - Gửi context + query cho Gemini để trả lời
4. **Nếu Chat:**
   - Gửi trực tiếp cho Gemini để trả lời tự nhiên
5. **Lưu conversation history** (tối đa 5 lượt gần nhất)

## Lưu ý quan trọng

1. **API Keys:** Không commit API keys vào git. Sử dụng User Secrets hoặc Environment Variables
2. **Rate Limits:** Google Gemini có rate limits, cần xử lý retry logic
3. **Cost:** Mỗi API call có chi phí, cần monitor usage
4. **Embedding Storage:** SQL Server lưu embedding dưới dạng JSON string, có thể chuyển sang PostgreSQL với pgvector nếu cần performance tốt hơn
5. **Conversation History:** Hiện tại lưu trong memory, cần migrate sang database nếu muốn persistent

## Troubleshooting

### Lỗi: "Cannot connect to database"
- Kiểm tra connection string trong `appsettings.json`
- Đảm bảo SQL Server đang chạy

### Lỗi: "Invalid API key"
- Kiểm tra API keys trong `appsettings.json`
- Đảm bảo API keys có quyền truy cập Gemini API

### Embeddings không được tạo
- Chạy lại script `generate_embeddings.py`
- Kiểm tra products có Description không null

## Next Steps

1. Thêm conversation history vào database
2. Thêm caching cho embeddings
3. Thêm logging và monitoring
4. Tối ưu performance với vector database (PostgreSQL + pgvector)
5. Thêm UI chat widget vào frontend

