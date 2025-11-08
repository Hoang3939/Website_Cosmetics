-- Script: Tìm ProductId của sản phẩm Maybelline SuperStay Matte Ink

USE WebsiteCosmetic;
GO

-- Tìm tất cả sản phẩm Maybelline
SELECT 
    ProductId,
    Name,
    Slug,
    Price
FROM dbo.Product
WHERE Name LIKE '%SuperStay%' OR Name LIKE '%Maybelline%'
ORDER BY CreatedAt DESC;

-- Hoặc xem tất cả sản phẩm
SELECT 
    ROW_NUMBER() OVER (ORDER BY CreatedAt DESC) AS [STT],
    ProductId,
    Name,
    Slug,
    Price
FROM dbo.Product
ORDER BY CreatedAt DESC;

PRINT '';
PRINT '=====================================================';
PRINT 'Copy ProductId của sản phẩm Maybelline SuperStay';
PRINT 'Paste vào file: 03_Insert_Maybelline_Product10_Images.sql';
PRINT 'Dòng: DECLARE @ProductId UNIQUEIDENTIFIER = ''...''';
PRINT '=====================================================';

