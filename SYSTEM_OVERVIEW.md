# Tổng quan hệ thống Website_Cosmetics

## 🎯 3 Hệ thống chính

Website_Cosmetics có 3 hệ thống AI/ML chính:

1. **🤖 Chatbot với RAG (Retrieval-Augmented Generation)**
2. **💡 Recommendation System (Content-Based Filtering)**
3. **✨ Virtual Try-On (AI Makeup Application)**

---

## 1. 🤖 CHATBOT VỚI RAG

### Mục đích
Chatbot thông minh có thể tìm kiếm sản phẩm dựa trên câu hỏi của người dùng và trả lời tự nhiên.

### Luồng chính

```
User Input (Frontend)
    ↓
ChatController.cs (API Endpoint)
    ↓
RAGService.cs (Core Logic)
    ↓
    ├─→ DecideActionAsync() → Gemini API (Decision Model)
    │   └─→ Quyết định: RAG hoặc Chat
    │
    ├─→ Nếu RAG:
    │   ├─→ RetrieveProductsAsync() → Tìm sản phẩm tương tự
    │   │   ├─→ GenerateEmbeddingAsync() → Gemini Embedding API
    │   │   └─→ CosineSimilarity() → So sánh embeddings
    │   └─→ ChatWithGeminiAsync() → Gemini Chat API (với context)
    │
    └─→ Nếu Chat:
        └─→ ChatWithGeminiAsync() → Gemini Chat API (trực tiếp)
```

### Entry Points

**Frontend:**
- `wwwroot/public/js/components/chatbot.js` - Chatbot widget UI
- `wwwroot/public/css/components/chatbot.css` - Styling
- `Views/Shared/_Layout.cshtml` - Tích hợp chatbot vào tất cả pages

**Backend:**
- `Controllers/ChatController.cs` - API endpoint `/api/chat`
- `Services/RAGService.cs` - Core RAG logic
- `Services/IRAGService.cs` - Interface

### API Endpoints

```
POST /api/chat
{
  "message": "Which lipsticks are under $35?",
  "sessionId": "optional"
}

Response:
{
  "response": "Here are some lipsticks...",
  "action": "rag",
  "products": [...]
}
```

### Database
- `Product.Embedding` (NVARCHAR(MAX)) - Lưu embedding vectors dưới dạng JSON

### Scripts
- `scripts/generate_embeddings.py` - Generate embeddings cho products

---

## 2. 💡 RECOMMENDATION SYSTEM

### Mục đích
Gợi ý sản phẩm tương tự dựa trên ingredients (content-based filtering).

### Luồng chính

```
Product Details Page (Frontend)
    ↓
JavaScript: loadRecommendations(productId)
    ↓
RecommendationsController.cs (API Endpoint)
    ↓
RecommendationService.cs (Core Logic)
    ↓
    ├─→ GetRecommendationsAsync(productId)
    │   ├─→ TokenizeIngredients() → Chia ingredients thành tokens
    │   ├─→ CreateOneHotVector() → Tạo binary vectors
    │   ├─→ CosineSimilarity() → Tính độ tương đồng
    │   └─→ Top K products → Trả về K sản phẩm tương tự nhất
    │
    └─→ Cache results → Lưu vào memory cache
```

### Entry Points

**Frontend:**
- `Views/Products/Details.cshtml` - Product details page
  - Section "You May Also Like" (line ~452)
  - JavaScript function `loadRecommendations()` (line ~900)

**Backend:**
- `Controllers/RecommendationsController.cs` - API endpoint `/api/recommendations/{productId}`
- `Services/RecommendationService.cs` - Core recommendation logic
- `Services/IRecommendationService.cs` - Interface

### API Endpoints

```
GET /api/recommendations/{productId}?topK=5

Response:
{
  "productId": 1,
  "recommendations": [
    {
      "productId": 3,
      "name": "Product Name",
      "price": 500000,
      "similarity": 0.85,
      "slug": "product-slug",
      "imageUrl": "/path/to/image.jpg"
    }
  ],
  "count": 5
}
```

### Database
- `Product.Ingredients` (TEXT) - Lưu danh sách ingredients

### Algorithm
1. **Tokenize**: Chia ingredients thành các từ riêng lẻ
2. **One-Hot Encoding**: Tạo vector binary (1 = có ingredient, 0 = không có)
3. **Cosine Similarity**: Tính độ tương đồng giữa các products
4. **Top K**: Trả về K sản phẩm tương tự nhất

---

## 3. ✨ VIRTUAL TRY-ON

### Mục đích
Cho phép người dùng thử makeup ảo bằng cách upload ảnh và áp dụng makeup lên khuôn mặt.

### Luồng chính

```
Product Details Page
    ↓
User clicks "Virtual Try-On" button
    ↓
VirtualMakeupService.cs (Backend Service)
    ↓
    ├─→ Call PSGAN API (Python service)
    │   └─→ http://localhost:5000 (PSGAN service)
    │
    └─→ Return result image
```

### Entry Points

**Frontend:**
- `Views/Products/Details.cshtml` - Virtual Try-On modal (line ~465)
- `wwwroot/public/js/components/virtual-tryon.js` - JavaScript handler
- `wwwroot/public/css/components/virtual-tryon.css` - Styling

**Backend:**
- `Services/VirtualMakeupService.cs` - Service gọi PSGAN API
- `Controllers/ProductsController.cs` - Có thể có endpoints liên quan

### External Service
- **PSGAN API**: `http://localhost:5000` (Python service từ PSGAN-master project)

### Configuration
- `appsettings.json` → `PsganApi.Url`: `http://localhost:5000`

---

## 📁 Cấu trúc Files chính

### Entry Point chính
```
Program.cs (Application Startup)
    ├─→ Đăng ký Services
    │   ├─→ IRAGService → RAGService
    │   ├─→ IRecommendationService → RecommendationService
    │   └─→ VirtualMakeupService
    │
    └─→ Configure Middleware
        ├─→ Session
        ├─→ Authentication
        └─→ Routing
```

### Controllers (API Entry Points)

```
Controllers/
├── ChatController.cs          → /api/chat (RAG Chatbot)
├── RecommendationsController.cs → /api/recommendations/{id} (Recommendations)
└── ProductsController.cs      → /Products/Details/{id} (Virtual Try-On)
```

### Services (Business Logic)

```
Services/
├── RAGService.cs              → RAG logic (embeddings, similarity, chat)
├── RecommendationService.cs   → Recommendation logic (ingredients similarity)
└── VirtualMakeupService.cs    → PSGAN API integration
```

### Frontend Components

```
wwwroot/public/
├── js/components/
│   ├── chatbot.js            → Chatbot widget
│   └── virtual-tryon.js      → Virtual Try-On handler
└── css/components/
    ├── chatbot.css           → Chatbot styling
    └── virtual-tryon.css    → Virtual Try-On styling
```

### Views

```
Views/
├── Shared/
│   └── _Layout.cshtml        → Tích hợp chatbot widget
└── Products/
    └── Details.cshtml        → Product details + Recommendations + Virtual Try-On
```

---

## 🔄 Luồng hoạt động tổng thể

### 1. User truy cập Website
```
Browser → _Layout.cshtml → Chatbot widget hiển thị
```

### 2. User chat với bot
```
User types message
    ↓
chatbot.js → POST /api/chat
    ↓
ChatController → RAGService
    ↓
Decision Model → RAG hoặc Chat
    ↓
Response → Display in chatbot widget
```

### 3. User xem Product Details
```
/Products/Details/{id}
    ↓
ProductsController.Details()
    ↓
Details.cshtml renders
    ↓
JavaScript: loadRecommendations(id)
    ↓
GET /api/recommendations/{id}
    ↓
RecommendationService → Similar products
    ↓
Display "You May Also Like" section
```

### 4. User thử Virtual Try-On
```
User clicks "Virtual Try-On" button
    ↓
virtual-tryon.js → Upload image
    ↓
VirtualMakeupService → Call PSGAN API
    ↓
PSGAN service processes image
    ↓
Return result → Display in modal
```

---

## 🗂️ File quan trọng nhất

### 1. Program.cs
**Vai trò:** Entry point của ứng dụng, đăng ký tất cả services

**Nội dung:**
- Đăng ký `IRAGService`, `IRecommendationService`, `VirtualMakeupService`
- Configure middleware (Session, Auth, Routing)
- Map routes

### 2. Controllers
- **ChatController.cs** → Entry point cho Chatbot
- **RecommendationsController.cs** → Entry point cho Recommendations
- **ProductsController.cs** → Entry point cho Product Details & Virtual Try-On

### 3. Services
- **RAGService.cs** → Core logic cho RAG
- **RecommendationService.cs** → Core logic cho Recommendations
- **VirtualMakeupService.cs** → Integration với PSGAN

### 4. Frontend
- **chatbot.js** → Chatbot UI logic
- **Details.cshtml** → Product page với Recommendations
- **_Layout.cshtml** → Tích hợp chatbot vào tất cả pages

---

## 📊 So sánh 3 hệ thống

| Feature | Chatbot (RAG) | Recommendation | Virtual Try-On |
|---------|---------------|----------------|----------------|
| **Input** | User message | Product ID | User image + Product |
| **Method** | Embedding similarity | Ingredients similarity | PSGAN AI model |
| **Output** | Text response + Products | Similar products list | Makeup result image |
| **Location** | Chatbot widget (all pages) | Product Details page | Product Details modal |
| **API** | `/api/chat` | `/api/recommendations/{id}` | PSGAN service |
| **Database** | `Product.Embedding` | `Product.Ingredients` | N/A (external service) |

---

## 🚀 Quick Start

### 1. Setup RAG
```bash
# Generate embeddings
cd scripts
python generate_embeddings.py

# Test chatbot
POST /api/chat
```

### 2. Setup Recommendations
```bash
# Ensure products have ingredients data
# Test API
GET /api/recommendations/1?topK=5
```

### 3. Setup Virtual Try-On
```bash
# Start PSGAN service (from PSGAN-master)
cd PSGAN-master
python app.py

# Test on Product Details page
```

---

## 📝 Documentation Files

1. **RAG_SETUP_GUIDE.md** - Hướng dẫn setup RAG
2. **RECOMMENDATION_SYSTEM_GUIDE.md** - Hướng dẫn Recommendation System
3. **SYSTEM_OVERVIEW.md** (file này) - Tổng quan toàn bộ hệ thống

---

## 🔗 Dependencies

### External Services
- **Google Gemini API** - Cho RAG (embeddings, chat, decision)
- **PSGAN Service** - Cho Virtual Try-On

### Database
- **SQL Server** - Lưu products, embeddings, ingredients

### Python Scripts
- `scripts/generate_embeddings.py` - Generate embeddings

---

## 💡 Best Practices

1. **Caching**: Recommendations được cache để tăng performance
2. **Error Handling**: Tất cả services đều có try-catch và logging
3. **Async/Await**: Tất cả I/O operations đều async
4. **Separation of Concerns**: Mỗi service có trách nhiệm riêng biệt







