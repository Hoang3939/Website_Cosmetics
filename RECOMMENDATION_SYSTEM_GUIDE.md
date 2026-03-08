# Hướng dẫn áp dụng Recommendation System vào Website_Cosmetics

## Tổng quan

Recommendation system này sử dụng **Content-Based Filtering** dựa trên **ingredients** của sản phẩm, tương tự như project Cosmetic.

## Kiến trúc

```
Product Ingredients → Tokenize → One-Hot Encoding → Cosine Similarity → Top K Recommendations
```

## Cách hoạt động

1. **Tokenize Ingredients**: Chia ingredients thành các từ riêng lẻ
2. **One-Hot Encoding**: Tạo vector binary cho mỗi product (1 = có ingredient, 0 = không có)
3. **Cosine Similarity**: Tính độ tương đồng giữa các products
4. **Top K Recommendations**: Trả về K sản phẩm tương tự nhất

## So sánh với RAG

| Feature | RAG (Hiện tại) | Recommendation System |
|---------|----------------|----------------------|
| **Input** | User query (text) | Product ID (sản phẩm đang xem) |
| **Method** | Embedding similarity | Ingredients similarity |
| **Use Case** | Tìm kiếm sản phẩm theo câu hỏi | Gợi ý sản phẩm tương tự |
| **Output** | Products matching query | Similar products to current product |

## Tích hợp

### 1. API Endpoint

**GET** `/api/recommendations/{productId}?topK=5`

Trả về danh sách sản phẩm tương tự dựa trên ingredients.

### 2. Sử dụng trong Product Details Page

Hiển thị "You may also like" section với các sản phẩm được recommend.

### 3. Kết hợp với RAG

- **RAG**: Tìm kiếm sản phẩm theo câu hỏi của user
- **Recommendation**: Gợi ý sản phẩm tương tự khi user xem một sản phẩm cụ thể

## Lưu ý

- Cần có ingredients data trong database
- Performance: Có thể cache similarity matrix để tăng tốc
- Có thể kết hợp thêm: category, brand, price range

