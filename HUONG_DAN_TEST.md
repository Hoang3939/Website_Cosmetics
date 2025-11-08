# HƯỚNG DẪN TEST TÍNH NĂNG VIRTUAL TRY-ON

## Bước 1: Chuẩn bị Database

### 1.1. Chạy script SQL để thêm cột mới

1. Mở **SQL Server Management Studio (SSMS)**
2. Connect vào database `WebsiteCosmetic`
3. Mở file: `D:\Website_Cosmetics\Database\01_Add_IsMakeupReference_Column.sql`
4. Nhấn **Execute** (hoặc F5)
5. Kiểm tra kết quả - sẽ thấy thông báo "Đã thêm cột IsMakeupReference"

### 1.2. Upload ảnh và cập nhật database

**Option A: Upload ảnh lên server/cloud storage**
1. Upload các ảnh từ folder Maybelline lên server
2. Lưu URL của các ảnh

**Option B: Dùng ảnh local (cho test)**
1. Copy ảnh vào folder: `D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\wwwroot\images\products\`
2. URL sẽ là: `/images/products/ten-anh.jpg`

**Sau đó chạy SQL để insert/update:**

```sql
USE WebsiteCosmetic;
GO

-- Giả sử ProductId của sản phẩm Maybelline là: '...'
DECLARE @ProductId UNIQUEIDENTIFIER = '...'; -- THAY ĐỔI ID NÀY

-- Insert ảnh người mẫu full mặt (makeup reference)
INSERT INTO dbo.ProductImage (ProductImageId, ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(NEWID(), @ProductId, '/images/products/maybelline-model-full.jpg', 0, 1, GETDATE());

-- Insert ảnh môi mẫu (makeup reference)  
INSERT INTO dbo.ProductImage (ProductImageId, ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(NEWID(), @ProductId, '/images/products/maybelline-lips-1.webp', 0, 1, GETDATE());

-- Xem kết quả
SELECT * FROM dbo.ProductImage WHERE ProductId = @ProductId;
```

**Hoặc nếu đã có ảnh trong database, chỉ cần update:**

```sql
-- Update ảnh có ID = '...' thành makeup reference
UPDATE dbo.ProductImage 
SET IsMakeupReference = 1 
WHERE ProductImageId = '...'; -- THAY ĐỔI ID NÀY
```

---

## Bước 2: Khởi động PSGAN API

1. Mở **PowerShell**
2. Chạy lệnh:

```powershell
cd D:\PSGAN-master
C:\Users\Hoang\VirtualMakeup\.venv\Scripts\python.exe api_server_v2.py
```

3. Đợi đến khi thấy:
```
✅ Model đã sẵn sàng!
🚀 Server starting on http://0.0.0.0:5000
```

4. **GIỮ CỬA SỔ NÀY MỞ** - đừng đóng lại

---

## Bước 3: Test API trực tiếp (Optional)

Trước khi test trên website, bạn có thể test API trực tiếp:

### 3.1. Chuẩn bị ảnh test

1. Tạo folder: `D:\Test_Images\`
2. Copy vào:
   - Ảnh của bạn (chưa makeup): `my_photo.jpg`
   - Ảnh người mẫu full mặt: `model_full_face.jpg`
   - Ảnh môi mẫu: `model_lips_only.jpg`

### 3.2. Chỉnh sửa script test

1. Mở file: `D:\PSGAN-master\test_api.py`
2. Tìm dòng:
```python
CUSTOMER_PHOTO = r"D:\Test_Images\my_photo.jpg"
MAKEUP_STYLE_FULL = r"D:\Test_Images\model_full_face.jpg"
MAKEUP_STYLE_LIPS = r"D:\Test_Images\model_lips_only.jpg"
```
3. Thay đổi đường dẫn cho đúng với file của bạn

### 3.3. Chạy test

```powershell
cd D:\PSGAN-master
C:\Users\Hoang\VirtualMakeup\.venv\Scripts\python.exe test_api.py
```

Kết quả sẽ được lưu trong folder: `D:\PSGAN-master\test_results\`

---

## Bước 4: Chạy Website ASP.NET

1. Mở **Visual Studio** hoặc **Rider**
2. Mở solution: `D:\Website_Cosmetics\Website_Cosmetics.sln`
3. Nhấn **Run** (F5) để chạy website
4. Website sẽ mở tại: `http://localhost:5210` (hoặc port khác)

---

## Bước 5: Test trên Website

### 5.1. Vào trang chi tiết sản phẩm

1. Truy cập: `http://localhost:5210/Products`
2. Chọn sản phẩm Maybelline (hoặc sản phẩm bạn đã setup makeup reference)
3. Click vào để xem chi tiết

### 5.2. Test chức năng Virtual Try-On

1. Tìm và click nút **"Thử son ảo"** hoặc **"Virtual Try-On"**
2. Modal sẽ hiện ra
3. Upload ảnh của bạn (kéo thả hoặc click để chọn)
4. Click **"Apply Makeup"**
5. Đợi xử lý (có thể mất 10-30 giây)
6. Xem kết quả:
   - Ảnh gốc bên trái
   - Ảnh đã makeup bên phải
7. Click **"Download"** để tải ảnh về
8. Click **"Try Again"** để thử lại với ảnh khác
9. F5 trang web → ảnh kết quả sẽ mất (không lưu đâu cả)

---

## So sánh kết quả: Full Face vs Lips Only

### Test 1: Ảnh người mẫu full mặt làm reference
- **Ưu điểm**: 
  - Có context toàn bộ khuôn mặt
  - Hiệu ứng makeup tự nhiên hơn
- **Nhược điểm**: 
  - Có thể bị ảnh hưởng bởi màu da, ánh sáng của người mẫu

### Test 2: Ảnh chỉ có môi làm reference
- **Ưu điểm**: 
  - Tập trung vào màu son
  - Không bị nhiễu bởi yếu tố khác
- **Nhược điểm**: 
  - Có thể thiếu context
  - Kết quả có thể kém tự nhiên hơn

**Khuyến nghị**: Test cả hai và chọn loại ảnh nào cho kết quả tốt hơn!

---

## Troubleshooting

### Lỗi: "Sản phẩm chưa có ảnh mẫu makeup"
→ Kiểm tra lại database, đảm bảo có ít nhất 1 ảnh với `IsMakeupReference = 1`

### Lỗi: "Cannot connect to API"
→ Kiểm tra PSGAN API có đang chạy không (port 5000)

### Lỗi: "No face detected"
→ Ảnh upload không có khuôn mặt rõ ràng, thử ảnh khác

### Website không gọi được API
→ Kiểm tra `appsettings.json`, đảm bảo `PsganApi.Url = "http://localhost:5000"`

---

## Lưu ý quan trọng

1. **PSGAN API phải chạy trước** khi test website
2. Ảnh kết quả **KHÔNG được lưu** vào database hay disk
3. Ảnh chỉ tồn tại trong **RAM** khi xử lý, và **trả về browser**
4. F5 trang web → ảnh mất
5. Port 5000 phải available (không bị process khác chiếm)

---

## Các file đã tạo/chỉnh sửa

### Database Scripts
- `D:\Website_Cosmetics\Database\01_Add_IsMakeupReference_Column.sql`
- `D:\Website_Cosmetics\Database\02_Sample_Data_Maybelline_Product.sql`

### Backend (C#)
- `D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\Models\ProductImage.cs`
  - ✅ Đã thêm property `IsMakeupReference`
- `D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\Controllers\ProductsController.cs`
  - ✅ Đã update để dùng ảnh makeup reference

### Frontend (đã có sẵn từ trước)
- `D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\Views\Products\Details.cshtml`
- `D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\wwwroot\public\js\components\virtual-tryon.js`
- `D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\wwwroot\public\css\components\virtual-tryon.css`

### PSGAN API
- `D:\PSGAN-master\api_server_v2.py` (server chính)
- `D:\PSGAN-master\test_api.py` (script test)
- `D:\PSGAN-master\START_API.md` (hướng dẫn)

---

## Kết luận

Sau khi test xong, nếu thấy OK:
1. Lưu lại kết quả test (screenshot, ảnh output)
2. Quyết định dùng ảnh full face hay lips only làm makeup reference
3. Cập nhật database production
4. Deploy lên server

Chúc bạn test thành công! 🎉

