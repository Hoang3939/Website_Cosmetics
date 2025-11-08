-- =============================================
-- Product Variant Examples
-- =============================================
-- File này minh họa cách sử dụng bảng ProductVariant
-- cho các loại sản phẩm khác nhau

USE WebsiteCosmetic;
GO

-- =============================================
-- VÍ DỤ 1: SON MÔI - Chỉ có màu sắc, không có size
-- =============================================
-- Product: The Kissu Lip Tint SPF 25 Hydrating Tinted Lip Sunscreen
-- Brand: Tatcha
-- Category: Lip Balms & Treatments

-- Giả sử ProductId = 1 (The Kissu Lip Tint)

-- Variant 1: Midnight Lily (màu tím)
INSERT INTO ProductVariant (
    ProductId, 
    VariantName, 
    ColorName, 
    ColorCode, 
    ColorFamily,
    Size,           -- NULL vì son này chỉ có 1 size duy nhất
    SizeValue,      -- NULL
    SizeUnit,       -- NULL
    VariantType,    -- NULL
    SKU, 
    Price,          -- NULL = dùng BasePrice từ Product
    Stock, 
    IsDefault,      -- TRUE cho variant đầu tiên
    DisplayOrder
) VALUES (
    1,
    N'Midnight Lily',
    N'Midnight Lily',
    '#6B2C4F',
    N'Purple',
    NULL,
    NULL,
    NULL,
    NULL,
    'TATCHA-KISSU-ML',
    NULL,
    50,
    1,  -- Variant mặc định
    1
);

-- Variant 2: Camellia (màu đỏ)
INSERT INTO ProductVariant (
    ProductId, VariantName, ColorName, ColorCode, ColorFamily,
    Size, SizeValue, SizeUnit, VariantType,
    SKU, Price, Stock, IsDefault, DisplayOrder
) VALUES (
    1, N'Camellia', N'Camellia', '#8B3A3A', N'Red',
    NULL, NULL, NULL, NULL,
    'TATCHA-KISSU-CM', NULL, 45, 0, 2
);

-- Variant 3: Plum Blossom (màu hồng)
INSERT INTO ProductVariant (
    ProductId, VariantName, ColorName, ColorCode, ColorFamily,
    Size, SizeValue, SizeUnit, VariantType,
    SKU, Price, Stock, IsDefault, DisplayOrder
) VALUES (
    1, N'Plum Blossom', N'Plum Blossom', '#C17B7B', N'Pink',
    NULL, NULL, NULL, NULL,
    'TATCHA-KISSU-PB', NULL, 60, 0, 3
);

-- Thêm ảnh cho từng màu son
-- Ảnh cho Midnight Lily
INSERT INTO ProductVariantImage (VariantId, Url, AltText, IsPrimary, DisplayOrder)
VALUES 
    (1, '/images/products/tatcha-kissu-midnight-lily-1.jpg', 'Tatcha Kissu Midnight Lily - Front View', 1, 1),
    (1, '/images/products/tatcha-kissu-midnight-lily-2.jpg', 'Tatcha Kissu Midnight Lily - Swatch', 0, 2),
    (1, '/images/products/tatcha-kissu-midnight-lily-3.jpg', 'Tatcha Kissu Midnight Lily - On Lips', 0, 3);

-- Ảnh cho Camellia
INSERT INTO ProductVariantImage (VariantId, Url, AltText, IsPrimary, DisplayOrder)
VALUES 
    (2, '/images/products/tatcha-kissu-camellia-1.jpg', 'Tatcha Kissu Camellia - Front View', 1, 1),
    (2, '/images/products/tatcha-kissu-camellia-2.jpg', 'Tatcha Kissu Camellia - Swatch', 0, 2);

-- =============================================
-- VÍ DỤ 2: CHAI XỊT - Chỉ có size, không có màu
-- =============================================
-- Product: Mario Badescu Facial Spray with Aloe, Herbs and Rosewater
-- Brand: Mario Badescu
-- Category: Face Mists & Essences

-- Giả sử ProductId = 2

-- Variant 1: Size 59ml (2 oz)
INSERT INTO ProductVariant (
    ProductId, VariantName, 
    ColorName,      -- NULL vì không có màu
    ColorCode,      -- NULL
    ColorFamily,    -- NULL
    Size, SizeValue, SizeUnit,
    VariantType,
    SKU, Price, CompareAtPrice, Stock, IsDefault, DisplayOrder
) VALUES (
    2, N'59ml (2 oz)',
    NULL, NULL, NULL,
    N'59ml (2 oz)', 59, 'ml',
    NULL,
    'MB-SPRAY-59ML', 7.00, 10.00, 150, 0, 1
);

-- Variant 2: Size 118ml (4 oz) - BEST SELLER
INSERT INTO ProductVariant (
    ProductId, VariantName, 
    ColorName, ColorCode, ColorFamily,
    Size, SizeValue, SizeUnit,
    VariantType,
    SKU, Price, CompareAtPrice, Stock, IsDefault, DisplayOrder
) VALUES (
    2, N'118ml (4 oz)',
    NULL, NULL, NULL,
    N'118ml (4 oz)', 118, 'ml',
    NULL,
    'MB-SPRAY-118ML', 12.00, 15.00, 200, 1, 2  -- Variant mặc định
);

-- Variant 3: Size 236ml (8 oz) - VALUE SIZE
INSERT INTO ProductVariant (
    ProductId, VariantName, 
    ColorName, ColorCode, ColorFamily,
    Size, SizeValue, SizeUnit,
    VariantType,
    SKU, Price, CompareAtPrice, Stock, IsDefault, DisplayOrder
) VALUES (
    2, N'236ml (8 oz)',
    NULL, NULL, NULL,
    N'236ml (8 oz)', 236, 'ml',
    NULL,
    'MB-SPRAY-236ML', 20.00, 28.00, 80, 0, 3
);

-- Ảnh cho chai xịt (có thể dùng chung hoặc riêng cho từng size)
INSERT INTO ProductVariantImage (VariantId, Url, AltText, IsPrimary, DisplayOrder)
VALUES 
    (4, '/images/products/mb-spray-59ml-1.jpg', 'Mario Badescu Spray 59ml', 1, 1),
    (5, '/images/products/mb-spray-118ml-1.jpg', 'Mario Badescu Spray 118ml', 1, 1),
    (6, '/images/products/mb-spray-236ml-1.jpg', 'Mario Badescu Spray 236ml', 1, 1);

-- =============================================
-- VÍ DỤ 3: PHẤN MÁ - Có cả màu VÀ size
-- =============================================
-- Product: NARS Blush
-- Brand: NARS
-- Category: Blush

-- Giả sử ProductId = 3

-- Variant 1: Orgasm - Travel Size
INSERT INTO ProductVariant (
    ProductId, VariantName, 
    ColorName, ColorCode, ColorFamily,
    Size, SizeValue, SizeUnit,
    VariantType,
    SKU, Price, Stock, IsDefault, DisplayOrder
) VALUES (
    3, N'Orgasm - Travel Size 3.5g',
    N'Orgasm', '#FF8FA3', N'Pink',
    N'3.5g', 3.5, 'g',
    NULL,
    'NARS-BLUSH-ORG-3.5G', 19.00, 30, 0, 1
);

-- Variant 2: Orgasm - Full Size
INSERT INTO ProductVariant (
    ProductId, VariantName, 
    ColorName, ColorCode, ColorFamily,
    Size, SizeValue, SizeUnit,
    VariantType,
    SKU, Price, Stock, IsDefault, DisplayOrder
) VALUES (
    3, N'Orgasm - Full Size 4.8g',
    N'Orgasm', '#FF8FA3', N'Pink',
    N'4.8g', 4.8, 'g',
    NULL,
    'NARS-BLUSH-ORG-4.8G', 30.00, 50, 1, 2  -- Variant mặc định
);

-- Variant 3: Deep Throat - Travel Size
INSERT INTO ProductVariant (
    ProductId, VariantName, 
    ColorName, ColorCode, ColorFamily,
    Size, SizeValue, SizeUnit,
    VariantType,
    SKU, Price, Stock, IsDefault, DisplayOrder
) VALUES (
    3, N'Deep Throat - Travel Size 3.5g',
    N'Deep Throat', '#F4A6B2', N'Pink',
    N'3.5g', 3.5, 'g',
    NULL,
    'NARS-BLUSH-DT-3.5G', 19.00, 25, 0, 3
);

-- Variant 4: Deep Throat - Full Size
INSERT INTO ProductVariant (
    ProductId, VariantName, 
    ColorName, ColorCode, ColorFamily,
    Size, SizeValue, SizeUnit,
    VariantType,
    SKU, Price, Stock, IsDefault, DisplayOrder
) VALUES (
    3, N'Deep Throat - Full Size 4.8g',
    N'Deep Throat', '#F4A6B2', N'Pink',
    N'4.8g', 4.8, 'g',
    NULL,
    'NARS-BLUSH-DT-4.8G', 30.00, 40, 0, 4
);

-- Ảnh cho phấn má (theo màu, có thể dùng chung cho các size)
INSERT INTO ProductVariantImage (VariantId, Url, AltText, IsPrimary, DisplayOrder)
VALUES 
    (7, '/images/products/nars-blush-orgasm-1.jpg', 'NARS Orgasm Blush', 1, 1),
    (7, '/images/products/nars-blush-orgasm-2.jpg', 'NARS Orgasm Swatch', 0, 2),
    (8, '/images/products/nars-blush-orgasm-1.jpg', 'NARS Orgasm Blush', 1, 1), -- Dùng chung ảnh
    (9, '/images/products/nars-blush-deepthroat-1.jpg', 'NARS Deep Throat Blush', 1, 1),
    (10, '/images/products/nars-blush-deepthroat-1.jpg', 'NARS Deep Throat Blush', 1, 1);

-- =============================================
-- VÍ DỤ 4: KEM DƯỠNG DA - Có Type và Size
-- =============================================
-- Product: CeraVe Moisturizing Cream
-- Brand: CeraVe
-- Category: Moisturizers

-- Giả sử ProductId = 4

-- Variant 1: For Normal to Dry Skin - 50ml
INSERT INTO ProductVariant (
    ProductId, VariantName, 
    ColorName, ColorCode, ColorFamily,
    Size, SizeValue, SizeUnit,
    VariantType,
    SKU, Price, Stock, IsDefault, DisplayOrder
) VALUES (
    4, N'For Normal to Dry Skin - 50ml',
    NULL, NULL, NULL,
    N'50ml', 50, 'ml',
    N'For Normal to Dry Skin',
    'CERAVE-MOIST-DRY-50ML', 12.00, 100, 1, 1
);

-- Variant 2: For Normal to Dry Skin - 177ml
INSERT INTO ProductVariant (
    ProductId, VariantName, 
    ColorName, ColorCode, ColorFamily,
    Size, SizeValue, SizeUnit,
    VariantType,
    SKU, Price, Stock, IsDefault, DisplayOrder
) VALUES (
    4, N'For Normal to Dry Skin - 177ml',
    NULL, NULL, NULL,
    N'177ml', 177, 'ml',
    N'For Normal to Dry Skin',
    'CERAVE-MOIST-DRY-177ML', 19.00, 80, 0, 2
);

-- Variant 3: For Oily Skin - 50ml
INSERT INTO ProductVariant (
    ProductId, VariantName, 
    ColorName, ColorCode, ColorFamily,
    Size, SizeValue, SizeUnit,
    VariantType,
    SKU, Price, Stock, IsDefault, DisplayOrder
) VALUES (
    4, N'For Oily Skin - 50ml',
    NULL, NULL, NULL,
    N'50ml', 50, 'ml',
    N'For Oily Skin',
    'CERAVE-MOIST-OILY-50ML', 12.00, 90, 0, 3
);

-- =============================================
-- QUERY EXAMPLES - Cách truy vấn dữ liệu
-- =============================================

-- 1. Lấy tất cả variants của một sản phẩm (Son môi)
SELECT 
    pv.VariantId,
    pv.VariantName,
    pv.ColorName,
    pv.ColorCode,
    pv.Size,
    COALESCE(pv.Price, p.BasePrice) AS FinalPrice,
    pv.Stock,
    pv.IsDefault
FROM ProductVariant pv
JOIN Product p ON pv.ProductId = p.ProductId
WHERE p.ProductId = 1
ORDER BY pv.DisplayOrder;

-- 2. Lấy variants theo màu (cho son môi)
SELECT 
    pv.VariantId,
    pv.ColorName,
    pv.ColorCode,
    pvi.Url AS ImageUrl,
    pvi.IsPrimary
FROM ProductVariant pv
LEFT JOIN ProductVariantImage pvi ON pv.VariantId = pvi.VariantId
WHERE pv.ProductId = 1
  AND pv.ColorName IS NOT NULL
ORDER BY pv.DisplayOrder, pvi.DisplayOrder;

-- 3. Lấy variants theo size (cho chai xịt)
SELECT 
    pv.VariantId,
    pv.Size,
    pv.SizeValue,
    pv.SizeUnit,
    pv.Price,
    pv.CompareAtPrice,
    CASE 
        WHEN pv.CompareAtPrice IS NOT NULL AND pv.CompareAtPrice > pv.Price 
        THEN ROUND((pv.CompareAtPrice - pv.Price) / pv.CompareAtPrice * 100, 0)
        ELSE 0 
    END AS DiscountPercent
FROM ProductVariant pv
WHERE pv.ProductId = 2
ORDER BY pv.SizeValue;

-- 4. Lấy variants có cả màu và size (phấn má)
SELECT 
    pv.VariantId,
    pv.VariantName,
    pv.ColorName,
    pv.ColorCode,
    pv.Size,
    pv.Price,
    pv.Stock,
    CASE WHEN pv.Stock < pv.LowStockThreshold THEN 1 ELSE 0 END AS IsLowStock
FROM ProductVariant pv
WHERE pv.ProductId = 3
ORDER BY pv.ColorName, pv.SizeValue;

-- 5. Lấy variant mặc định của sản phẩm
SELECT 
    p.ProductId,
    p.Name AS ProductName,
    pv.VariantId,
    pv.VariantName,
    COALESCE(pv.Price, p.BasePrice) AS Price,
    pvi.Url AS PrimaryImageUrl
FROM Product p
LEFT JOIN ProductVariant pv ON p.ProductId = pv.ProductId AND pv.IsDefault = 1
LEFT JOIN ProductVariantImage pvi ON pv.VariantId = pvi.VariantId AND pvi.IsPrimary = 1
WHERE p.ProductId = 1;

-- 6. Lấy tất cả ảnh của một variant cụ thể
SELECT 
    pvi.ImageId,
    pvi.Url,
    pvi.AltText,
    pvi.IsPrimary,
    pvi.DisplayOrder
FROM ProductVariantImage pvi
WHERE pvi.VariantId = 1
ORDER BY pvi.DisplayOrder;

-- 7. Thống kê variants theo màu sắc
SELECT 
    pv.ColorFamily,
    COUNT(*) AS VariantCount,
    SUM(pv.Stock) AS TotalStock,
    AVG(COALESCE(pv.Price, p.BasePrice)) AS AveragePrice
FROM ProductVariant pv
JOIN Product p ON pv.ProductId = p.ProductId
WHERE pv.ColorFamily IS NOT NULL
GROUP BY pv.ColorFamily
ORDER BY VariantCount DESC;

PRINT '=============================================';
PRINT 'Product Variant Examples Created Successfully!';
PRINT '=============================================';
PRINT 'Examples included:';
PRINT '1. Lipstick (Color only)';
PRINT '2. Facial Spray (Size only)';
PRINT '3. Blush (Color + Size)';
PRINT '4. Moisturizer (Type + Size)';
PRINT '';
PRINT 'Use the query examples at the end to retrieve data.';
PRINT '=============================================';

