-- Kiểm tra sản phẩm có tồn tại không
USE WebsiteCosmetic;
GO

DECLARE @ProductId UNIQUEIDENTIFIER = '031EA73B-C58C-4E9C-81A9-A7C0C0996C76';

-- Kiểm tra sản phẩm
SELECT 
    ProductId,
    Name,
    Slug,
    Price,
    IsActive,
    BrandId,
    CategoryId,
    CreatedAt
FROM dbo.Product
WHERE ProductId = @ProductId;

-- Nếu IsActive = 0 hoặc NULL, bật lên:
UPDATE dbo.Product 
SET IsActive = 1 
WHERE ProductId = @ProductId AND (IsActive = 0 OR IsActive IS NULL);

-- Xem lại
SELECT 
    ProductId,
    Name,
    IsActive
FROM dbo.Product
WHERE ProductId = @ProductId;

PRINT '✅ Kiểm tra xong! Nếu IsActive = 1 thì OK.';

GO

