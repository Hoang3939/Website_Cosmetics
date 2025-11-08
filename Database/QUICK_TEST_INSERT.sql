-- SCRIPT TEST NHANH - CẬP NHẬT MAKEUP REFERENCE
-- Dùng cho test tính năng Virtual Try-On

USE WebsiteCosmetic;
GO

-- =====================================================
-- BƯỚC 1: TÌM PRODUCT ID
-- =====================================================
-- Xem tất cả sản phẩm
SELECT 
    ProductId, 
    Name,
    BrandId
FROM dbo.Product
ORDER BY CreatedAt DESC;

-- Copy ProductId của sản phẩm bạn muốn test (ví dụ: Maybelline)
-- Paste vào dòng dưới:
DECLARE @ProductId UNIQUEIDENTIFIER = 'PASTE-PRODUCT-ID-O-DAY';

-- =====================================================
-- BƯỚC 2: XEM CÁC ẢNH HIỆN TẠI CỦA SẢN PHẨM
-- =====================================================
SELECT 
    ProductImageId,
    Url,
    IsCover AS [Ảnh bìa?],
    IsMakeupReference AS [Ảnh mẫu makeup?]
FROM dbo.ProductImage
WHERE ProductId = @ProductId;

-- =====================================================
-- BƯỚC 3: OPTION A - CẬP NHẬT ẢNH CŨ THÀNH MAKEUP REFERENCE
-- =====================================================
-- Nếu đã có ảnh trong database, chỉ cần update:

-- Xem ảnh nào muốn dùng làm makeup reference, copy ProductImageId
-- Paste vào dòng dưới và uncomment:

/*
UPDATE dbo.ProductImage 
SET IsMakeupReference = 1 
WHERE ProductImageId = 'PASTE-IMAGE-ID-O-DAY';
*/

-- =====================================================
-- BƯỚC 4: OPTION B - THÊM ẢNH MỚI (từ wwwroot)
-- =====================================================
-- Trước tiên copy ảnh vào: D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\wwwroot\images\products\
-- Ví dụ: maybelline-model.jpg

-- Sau đó uncomment và chạy:

/*
-- Thêm ảnh người mẫu full mặt
INSERT INTO dbo.ProductImage (ProductImageId, ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(NEWID(), @ProductId, '/images/products/maybelline-model.jpg', 0, 1, GETDATE());

-- Thêm ảnh môi mẫu
INSERT INTO dbo.ProductImage (ProductImageId, ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(NEWID(), @ProductId, '/images/products/maybelline-lips.webp', 0, 1, GETDATE());
*/

-- =====================================================
-- BƯỚC 5: KIỂM TRA KẾT QUẢ
-- =====================================================
SELECT 
    ProductImageId,
    Url,
    IsCover AS [Ảnh bìa?],
    IsMakeupReference AS [Ảnh mẫu makeup?],
    CreatedAt
FROM dbo.ProductImage
WHERE ProductId = @ProductId
ORDER BY IsMakeupReference DESC, IsCover DESC;

-- =====================================================
-- KẾT QUẢ MONG ĐỢI:
-- =====================================================
-- Phải có ít nhất 1 dòng với IsMakeupReference = 1
-- Đây là ảnh sẽ được dùng để transfer makeup cho khách hàng

GO

PRINT '===========================================';
PRINT 'HOÀN TẤT!';
PRINT 'Bây giờ có thể test trên website.';
PRINT '===========================================';

