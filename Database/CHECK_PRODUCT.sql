-- Kiểm tra sản phẩm Maybelline có active không?

USE WebsiteCosmetic;
GO

-- Kiểm tra sản phẩm
SELECT 
    ProductId,
    Name,
    Slug,
    Price,
    IsActive,
    CreatedAt
FROM dbo.Product
WHERE ProductId = '031EA73B-C58C-4E9C-81A9-A7C0C0996C76';

-- Nếu IsActive = 0, cần bật lên
-- Uncomment dòng dưới và chạy nếu IsActive = 0:

/*
UPDATE dbo.Product 
SET IsActive = 1 
WHERE ProductId = '031EA73B-C58C-4E9C-81A9-A7C0C0996C76';

PRINT '✅ Đã bật IsActive cho sản phẩm!';
*/

GO

