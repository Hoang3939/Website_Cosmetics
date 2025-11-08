-- Script: Insert dữ liệu mẫu cho sản phẩm Maybelline SuperStay Matte Ink
-- Lưu ý: Script này chỉ để tham khảo, bạn cần chỉnh sửa theo dữ liệu thực tế

USE WebsiteCosmetic;
GO

-- Giả sử ProductId = 1 (Bạn cần thay đổi theo ID thực tế của sản phẩm)
DECLARE @ProductId INT = 1;

-- Ví dụ: Thêm các ảnh cho sản phẩm
-- Lưu ý: Bạn cần upload ảnh lên server hoặc cloud storage trước, 
-- sau đó thay đổi URL trong script này

/*
-- Insert ảnh sản phẩm chính (cây son)
INSERT INTO dbo.ProductImage (ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(@ProductId, '/images/products/maybelline-superstay-135-product.jpg', 1, 0, GETDATE());

-- Insert ảnh người mẫu full mặt (dùng làm makeup reference)
INSERT INTO dbo.ProductImage (ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(@ProductId, '/images/products/maybelline-superstay-135-model-full.jpg', 0, 1, GETDATE());

-- Insert ảnh môi mẫu 1 (có thể dùng làm makeup reference)
INSERT INTO dbo.ProductImage (ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(@ProductId, '/images/products/maybelline-superstay-135-lips-1.webp', 0, 1, GETDATE());

-- Insert ảnh môi mẫu 2
INSERT INTO dbo.ProductImage (ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(@ProductId, '/images/products/maybelline-superstay-135-lips-2.webp', 0, 0, GETDATE());

-- Insert ảnh môi mẫu 3
INSERT INTO dbo.ProductImage (ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(@ProductId, '/images/products/maybelline-superstay-135-lips-3.webp', 0, 0, GETDATE());
*/

-- Hoặc nếu bạn đã có ảnh trong database, chỉ cần update IsMakeupReference = 1
-- Ví dụ: Update ảnh có ID = 5 thành ảnh makeup reference
/*
UPDATE dbo.ProductImage 
SET IsMakeupReference = 1 
WHERE ProductImageId = 5 AND ProductId = @ProductId;
*/

-- Xem tất cả ảnh của sản phẩm
SELECT 
    ProductImageId,
    ProductId,
    Url,
    IsCover AS [Ảnh bìa?],
    IsMakeupReference AS [Ảnh mẫu makeup?],
    CreatedAt
FROM dbo.ProductImage
WHERE ProductId = @ProductId
ORDER BY IsCover DESC, IsMakeupReference DESC, CreatedAt;

GO

