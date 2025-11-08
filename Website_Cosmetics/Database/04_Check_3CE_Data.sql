-- =============================================
-- Check 3CE Cashmere Hug Lipstick Data
-- =============================================
-- Script để kiểm tra dữ liệu vừa insert
-- =============================================

USE WebsiteCosmetic;
GO

-- Check Product
SELECT 
    ProductId,
    Name,
    Slug,
    BasePrice,
    BrandId,
    CategoryId,
    IsActive
FROM Product
WHERE Name = '3CE Cashmere Hug Lipstick';
GO

-- Check Variants
SELECT 
    v.VariantId,
    v.ProductId,
    v.VariantName,
    v.ColorName,
    v.ColorCode,
    v.SKU,
    v.Stock,
    v.IsDefault,
    v.DisplayOrder,
    COUNT(img.ImageId) AS ImageCount
FROM ProductVariant v
LEFT JOIN ProductVariantImage img ON v.VariantId = img.VariantId
WHERE v.ProductId = (SELECT ProductId FROM Product WHERE Name = '3CE Cashmere Hug Lipstick')
GROUP BY 
    v.VariantId,
    v.ProductId,
    v.VariantName,
    v.ColorName,
    v.ColorCode,
    v.SKU,
    v.Stock,
    v.IsDefault,
    v.DisplayOrder
ORDER BY v.DisplayOrder;
GO

-- Check Images (chi tiết)
SELECT 
    v.VariantName,
    v.ColorName,
    img.ImageId,
    img.Url,
    img.AltText,
    img.IsPrimary,
    img.DisplayOrder,
    CASE 
        WHEN img.Url LIKE '%model%' THEN '⭐ Virtual Try-On'
        WHEN img.IsPrimary = 1 THEN '📸 Main Image'
        ELSE '🖼️ Gallery'
    END AS ImageType
FROM ProductVariant v
INNER JOIN ProductVariantImage img ON v.VariantId = img.VariantId
WHERE v.ProductId = (SELECT ProductId FROM Product WHERE Name = '3CE Cashmere Hug Lipstick')
ORDER BY v.DisplayOrder, img.DisplayOrder;
GO

-- Check Virtual Try-On Images (chỉ các ảnh model)
SELECT 
    v.VariantName,
    v.ColorName,
    img.Url AS VirtualTryOnImageUrl,
    img.AltText
FROM ProductVariant v
INNER JOIN ProductVariantImage img ON v.VariantId = img.VariantId
WHERE v.ProductId = (SELECT ProductId FROM Product WHERE Name = '3CE Cashmere Hug Lipstick')
  AND img.Url LIKE '%model%'
ORDER BY v.DisplayOrder;
GO

PRINT '=============================================';
PRINT '✅ Data check completed!';
PRINT '=============================================';

