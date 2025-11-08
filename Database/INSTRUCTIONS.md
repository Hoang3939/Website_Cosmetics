# HƯỚNG DẪN FIX LỖI "KHÔNG CÓ ẢNH MẪU MAKEUP"

## Vấn đề
ProductId thay đổi sau mỗi lần refresh vì database có thể bị reset hoặc data được tạo lại.

## Giải pháp

### Bước 1: Tìm ProductId hiện tại của sản phẩm Maybelline SuperStay Matte Ink

Mở SQL Server Management Studio (SSMS) và chạy script:
```
D:\Website_Cosmetics\Database\04_Find_Maybelline_ProductId.sql
```

Hoặc copy đoạn SQL này vào SSMS:

```sql
USE WebsiteCosmetic;
GO

SELECT 
    ProductId,
    Name,
    Price,
    IsActive,
    (SELECT COUNT(*) FROM ProductImage WHERE ProductImage.ProductId = Product.ProductId) AS [Số ảnh],
    (SELECT COUNT(*) FROM ProductImage WHERE ProductImage.ProductId = Product.ProductId AND IsMakeupReference = 1) AS [Có makeup ref?]
FROM dbo.Product
WHERE Name LIKE '%Maybelline%SuperStay%'
   OR Name LIKE '%SuperStay%Matte%Ink%'
ORDER BY CreatedAt DESC;
```

### Bước 2: Copy ProductId từ kết quả

Ví dụ: `7ADE93DF-3D89-47FA-B490-58C6CA4C2706`

### Bước 3: Update script 03_Insert_Maybelline_Product10_Images.sql

Mở file:
```
D:\Website_Cosmetics\Database\03_Insert_Maybelline_Product10_Images.sql
```

Thay dòng 10:
```sql
DECLARE @ProductId UNIQUEIDENTIFIER = '031EA73B-C58C-4E9C-81A9-A7C0C0996C76';
```

Thành:
```sql
DECLARE @ProductId UNIQUEIDENTIFIER = 'PRODUCT_ID_MỚI_TỪ_BƯỚC_2';
```

### Bước 4: Chạy lại script 03

Chạy file `03_Insert_Maybelline_Product10_Images.sql` trong SSMS.

### Bước 5: Refresh website

Mở lại trang product detail và thử Virtual Try-On.

---

## Cách tránh ProductId thay đổi trong tương lai

### Option 1: Dùng ProductId cố định khi seed data
Trong file seed data (nếu có), thay vì dùng `NEWID()`, hãy dùng một GUID cố định:

```csharp
new Product 
{ 
    ProductId = Guid.Parse("7ADE93DF-3D89-47FA-B490-58C6CA4C2706"),
    Name = "Maybelline SuperStay Matte Ink",
    // ...
}
```

### Option 2: Dùng tên sản phẩm để tìm thay vì ProductId
Trong ProductsController, có thể tìm product bằng slug hoặc tên thay vì GUID.

### Option 3: Không reset database trong development
- Tắt `EnsureDeleted()` hoặc migrations reset trong `Program.cs`
- Chỉ dùng `EnsureCreated()` lần đầu

