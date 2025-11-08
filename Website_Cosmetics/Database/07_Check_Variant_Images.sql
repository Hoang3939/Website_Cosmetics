-- =============================================
-- Kiểm tra ảnh của Variant 115 và 107
-- =============================================

USE WebsiteCosmetic;
GO

-- Kiểm tra Variant 115
PRINT '========================================';
PRINT 'VARIANT 115:';
PRINT '========================================';

SELECT 
    v.VariantId,
    v.VariantName,
    v.ColorName,
    v.ProductId,
    p.Name AS ProductName,
    COUNT(pvi.ImageId) AS ImageCount
FROM ProductVariant v
LEFT JOIN Product p ON v.ProductId = p.ProductId
LEFT JOIN ProductVariantImage pvi ON v.VariantId = pvi.VariantId
WHERE v.VariantId = 115
GROUP BY v.VariantId, v.VariantName, v.ColorName, v.ProductId, p.Name;

-- Xem chi tiết ảnh của Variant 115
SELECT 
    pvi.ImageId,
    pvi.VariantId,
    pvi.Url,
    pvi.AltText,
    pvi.IsPrimary,
    pvi.DisplayOrder,
    pvi.CreatedAt
FROM ProductVariantImage pvi
WHERE pvi.VariantId = 115
ORDER BY pvi.IsPrimary DESC, pvi.DisplayOrder;

PRINT '';
PRINT '========================================';
PRINT 'VARIANT 107:';
PRINT '========================================';

-- Kiểm tra Variant 107
SELECT 
    v.VariantId,
    v.VariantName,
    v.ColorName,
    v.ProductId,
    p.Name AS ProductName,
    COUNT(pvi.ImageId) AS ImageCount
FROM ProductVariant v
LEFT JOIN Product p ON v.ProductId = p.ProductId
LEFT JOIN ProductVariantImage pvi ON v.VariantId = pvi.VariantId
WHERE v.VariantId = 107
GROUP BY v.VariantId, v.VariantName, v.ColorName, v.ProductId, p.Name;

-- Xem chi tiết ảnh của Variant 107
SELECT 
    pvi.ImageId,
    pvi.VariantId,
    pvi.Url,
    pvi.AltText,
    pvi.IsPrimary,
    pvi.DisplayOrder,
    pvi.CreatedAt
FROM ProductVariantImage pvi
WHERE pvi.VariantId = 107
ORDER BY pvi.IsPrimary DESC, pvi.DisplayOrder;

GO

