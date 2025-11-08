-- TÌM PRODUCTID CỦA MAYBELLINE SUPERSTAY MATTE INK
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

