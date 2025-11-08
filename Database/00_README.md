# HƯỚNG DẪN TẠO LẠI DATABASE VỚI GUID CỐ ĐỊNH

## 🎯 Mục đích
Script này giúp bạn tạo lại toàn bộ database với **GUID cố định**, đảm bảo ProductId và các ID khác **KHÔNG thay đổi** mỗi lần chạy lại.

## 📋 Các bước thực hiện

### Bước 1: Mở SQL Server Management Studio (SSMS)

Kết nối đến SQL Server instance của bạn.

### Bước 2: Chạy script 01_CreateTables.sql

```sql
-- File: 01_CreateTables.sql
-- Chức năng: Xóa database cũ và tạo lại toàn bộ cấu trúc bảng
```

**Cảnh báo:** Script này sẽ **XÓA TOÀN BỘ DATABASE CŨ**!

**Cách chạy:**
1. Mở file `01_CreateTables.sql` trong SSMS
2. Nhấn F5 hoặc click "Execute"
3. Kiểm tra output, phải thấy thông báo: "✅ Đã tạo xong tất cả các bảng"

### Bước 3: Chạy script 02_InsertData.sql

```sql
-- File: 02_InsertData.sql
-- Chức năng: Thêm dữ liệu mẫu với GUID cố định
```

**Cách chạy:**
1. Mở file `02_InsertData.sql` trong SSMS
2. Nhấn F5 hoặc click "Execute"
3. Kiểm tra output, phải thấy thông báo: "🎉 Đã thêm xong tất cả dữ liệu mẫu!"

### Bước 4: Kiểm tra dữ liệu

Chạy query kiểm tra:

```sql
USE WebsiteCosmetic;
GO

-- Kiểm tra sản phẩm Maybelline SuperStay Matte Ink
SELECT 
    p.ProductId,
    p.Name,
    p.Price,
    b.Name AS Brand,
    c.Name AS Category,
    (SELECT COUNT(*) FROM ProductImage WHERE ProductId = p.ProductId) AS [Số ảnh],
    (SELECT COUNT(*) FROM ProductImage WHERE ProductId = p.ProductId AND IsMakeupReference = 1) AS [Makeup Ref?]
FROM Product p
LEFT JOIN Brand b ON p.BrandId = b.BrandId
LEFT JOIN Category c ON p.CategoryId = c.CategoryId
WHERE p.ProductId = '7ADE93DF-3D89-47FA-B490-58C6CA4C2706';

-- Kiểm tra ảnh sản phẩm
SELECT 
    ProductImageId,
    Url,
    IsCover AS [Cover?],
    IsMakeupReference AS [Makeup Ref?]
FROM ProductImage
WHERE ProductId = '7ADE93DF-3D89-47FA-B490-58C6CA4C2706';
```

**Kết quả mong đợi:**
- 1 sản phẩm: Maybelline SuperStay Matte Ink #135
- 3 ảnh: 
  - `cay-son-superstay-135.jpg` (Cover = 1, MakeupRef = 0)
  - `nguoi-mau-superstay-135.jpg` (Cover = 0, MakeupRef = 1) ⭐
  - `moi-mau-superstay-135.jpg` (Cover = 0, MakeupRef = 0)

### Bước 5: Đảm bảo file ảnh đã được copy đúng

Kiểm tra folder:
```
D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\wwwroot\public\images\products\
```

Phải có 3 file:
- ✅ `cay-son-superstay-135.jpg`
- ✅ `nguoi-mau-superstay-135.jpg`
- ✅ `moi-mau-superstay-135.jpg`

### Bước 6: Cập nhật connection string (nếu cần)

Kiểm tra file `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=WebsiteCosmetic;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Bước 7: Khởi động website

1. Mở Terminal trong VS Code hoặc Cursor
2. Chạy lệnh:
```bash
cd D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics
dotnet run
```

3. Truy cập:
```
http://localhost:5210/Products/Details/7ADE93DF-3D89-47FA-B490-58C6CA4C2706
```

4. Click nút **"Virtual Try-On"** và test!

## 🔑 Thông tin quan trọng

### GUID CỐ ĐỊNH (KHÔNG ĐỔI)

**Sản phẩm test Virtual Try-On:**
- **ProductId:** `7ADE93DF-3D89-47FA-B490-58C6CA4C2706`
- **Name:** Maybelline SuperStay Matte Ink #135 - Globetrotter
- **URL:** `/Products/Details/7ADE93DF-3D89-47FA-B490-58C6CA4C2706`
- **Makeup Reference Image:** `/public/images/products/nguoi-mau-superstay-135.jpg`

**Brands (GUID cố định):**
- Maybelline: `11111111-1111-1111-1111-111111111111`
- L'Oréal Paris: `22222222-2222-2222-2222-222222222222`
- MAC Cosmetics: `33333333-3333-3333-3333-333333333333`
- NYX: `44444444-4444-4444-4444-444444444444`
- Revlon: `55555555-5555-5555-5555-555555555555`

**Categories (GUID cố định):**
- Lipstick: `AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA`
- Foundation: `BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB`
- Mascara: `CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC`
- Eyeshadow: `DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD`
- Blush: `EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE`

### Thông tin đăng nhập (Admin)
- **Username:** admin
- **Email:** admin@cosmetics.com
- **Password:** Admin@123 (đây chỉ là fake hash, bạn cần implement authentication đúng cách)

## ❓ Troubleshooting

### Lỗi: "Database 'WebsiteCosmetic' is being accessed by other users"
**Giải pháp:**
```sql
USE master;
GO
ALTER DATABASE WebsiteCosmetic SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
DROP DATABASE WebsiteCosmetic;
GO
```

Sau đó chạy lại script `01_CreateTables.sql`.

### Lỗi: "Cannot find the object... because it does not exist"
**Nguyên nhân:** Chưa chạy script `01_CreateTables.sql`

**Giải pháp:** Chạy lại từ đầu: `01_CreateTables.sql` → `02_InsertData.sql`

### Lỗi: Ảnh không hiển thị (404)
**Kiểm tra:**
1. File ảnh có trong folder `wwwroot/public/images/products/` không?
2. Tên file có đúng không? (không dấu, không khoảng trắng)
3. Website có đang chạy không?

### Lỗi: "Sản phẩm chưa có ảnh mẫu makeup"
**Nguyên nhân:** Không có ảnh nào có `IsMakeupReference = 1`

**Giải pháp:**
```sql
UPDATE ProductImage
SET IsMakeupReference = 1
WHERE ProductId = '7ADE93DF-3D89-47FA-B490-58C6CA4C2706'
  AND Url LIKE '%nguoi-mau-superstay-135%';
```

## 📝 Lưu ý

1. ✅ **GUID đã cố định** - Không cần lo lắng về việc ProductId thay đổi nữa!
2. ✅ Script an toàn để chạy lại nhiều lần (idempotent)
3. ✅ Tất cả dữ liệu đều có timestamp (CreatedAt, UpdatedAt)
4. ⚠️ **Luôn backup database** trước khi chạy script trong production!

## 🚀 Quick Start (TL;DR)

```sql
-- 1. Chạy trong SSMS:
-- Chọn file: 01_CreateTables.sql → Execute (F5)
-- Chọn file: 02_InsertData.sql → Execute (F5)

-- 2. Chạy website:
cd D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics
dotnet run

-- 3. Test Virtual Try-On:
-- http://localhost:5210/Products/Details/7ADE93DF-3D89-47FA-B490-58C6CA4C2706
```

---

✅ **Done!** Database đã sẵn sàng với GUID cố định!

