-- =============================================
-- Insert 3CE Cashmere Hug Lipstick Product Data
-- =============================================
-- Product: 3CE Cashmere Hug Lipstick
-- Brand: 3CE (3 Concept Eyes)
-- Category: Lipstick / Lip Products
-- =============================================
-- 
-- IMPORTANT NOTES:
-- 1. Script này CHẠY ĐƯỢC NHIỀU LẦN - sẽ xóa dữ liệu cũ và insert lại
-- 2. Ảnh phải có trong: wwwroot/public/images/products/3CE-Cashmere-Hug-Lipstick/
-- 3. Đường dẫn trong database là tương đối từ wwwroot: /public/images/products/...
-- 4. Virtual Try-On Images đã được đánh dấu rõ ràng với comment "VIRTUAL TRY-ON"
-- 
-- Virtual Try-On Images:
--   - #01 OAT: 3CE_L01_model1.png
--   - #03 YOUR SIDE: 3CE_L03_model1.jpg
--   - #04 TEXT ME: 3CE_L04_model1.jpeg
--   - #06 BUDDY: 3CE_L06_model2.jpg
--   - #08 HUSH RED: 3CE_L08_model1.png
-- =============================================

USE WebsiteCosmetic;
GO

SET NOCOUNT ON;

BEGIN TRANSACTION;

-- =============================================
-- Step 0: Delete existing data (if exists) - Để script có thể chạy nhiều lần
-- =============================================
DECLARE @ExistingProductId INT;
SELECT @ExistingProductId = ProductId FROM Product WHERE Name = '3CE Cashmere Hug Lipstick';

IF @ExistingProductId IS NOT NULL
BEGIN
    PRINT 'Deleting existing product data...';
    
    -- Delete images
    DELETE FROM ProductVariantImage 
    WHERE VariantId IN (SELECT VariantId FROM ProductVariant WHERE ProductId = @ExistingProductId);
    
    DELETE FROM ProductImage WHERE ProductId = @ExistingProductId;
    
    -- Delete variants
    DELETE FROM ProductVariant WHERE ProductId = @ExistingProductId;
    
    -- Delete product
    DELETE FROM Product WHERE ProductId = @ExistingProductId;
    
    PRINT 'Existing data deleted.';
END

-- =============================================
-- Step 1: Insert Brand (3CE)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Brand WHERE Name = '3CE')
BEGIN
    INSERT INTO Brand (Name, Country, IsActive)
    VALUES ('3CE', 'South Korea', 1);
    PRINT 'Brand 3CE inserted successfully';
END
ELSE
BEGIN
    PRINT 'Brand 3CE already exists';
END
GO

-- =============================================
-- Step 2: Insert Category (Lipstick)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Category WHERE Name = 'Lipstick')
BEGIN
    INSERT INTO Category (Name, Description, IsActive)
    VALUES ('Lipstick', 'Lipstick and lip color products', 1);
    PRINT 'Category Lipstick inserted successfully';
END
ELSE
BEGIN
    PRINT 'Category Lipstick already exists';
END
GO

-- =============================================
-- Step 3: Insert Main Product
-- =============================================
-- Get BrandId and CategoryId (sau GO statement, biến bị reset nên cần lấy lại)
DECLARE @BrandId INT = (SELECT BrandId FROM Brand WHERE Name = '3CE');
DECLARE @CategoryId INT = (SELECT CategoryId FROM Category WHERE Name = 'Lipstick');
DECLARE @ProductId INT;

INSERT INTO Product (
    Name,
    Slug,
    BasePrice,
    Description,
    Ingredients,
    Rating,
    ReviewCount,
    LikeCount,
    SPF,
    Size,
    Finish,
    IsActive,
    BrandId,
    CategoryId
)
VALUES (
    '3CE Cashmere Hug Lipstick',
    '3ce-cashmere-hug-lipstick',
    25.00,
    'A creamy, smooth lipstick with cashmere-like texture that hugs your lips for a comfortable, long-lasting finish.',
    'Paraffin, Beeswax, Lanolin, Mineral Oil, etc.',
    4.5,
    128,
    256,
    NULL,
    '3.5g',
    'Matte',
    1,
    @BrandId,
    @CategoryId
);

SET @ProductId = SCOPE_IDENTITY();
PRINT 'Product 3CE Cashmere Hug Lipstick inserted successfully. ProductId: ' + CAST(@ProductId AS NVARCHAR(10));
GO

-- =============================================
-- Step 4: Insert Product Variants (Colors)
-- =============================================
-- Get ProductId (sau GO statement, biến bị reset nên cần lấy lại)
DECLARE @ProductId INT = (SELECT ProductId FROM Product WHERE Name = '3CE Cashmere Hug Lipstick');
DECLARE @BrandId INT = (SELECT BrandId FROM Brand WHERE Name = '3CE');
DECLARE @CategoryId INT = (SELECT CategoryId FROM Category WHERE Name = 'Lipstick');

-- Base path for images (tương đối từ wwwroot)
-- NOTE: Folder name có dấu # và khoảng trắng, URL encode: %23 = #, %20 = space
DECLARE @ImageBasePath NVARCHAR(500) = '/public/images/products/3CE-Cashmere-Hug-Lipstick';

-- =============================================
-- Variant 1: #01 OAT
-- =============================================
DECLARE @VariantId_OAT INT;

INSERT INTO ProductVariant (
    ProductId,
    VariantName,
    ColorName,
    ColorCode,
    ColorFamily,
    SKU,
    Price,
    CompareAtPrice,
    Stock,
    IsDefault,
    DisplayOrder
)
VALUES (
    @ProductId,
    '#01 OAT',
    'OAT',
    '#E8D5B7',
    'Nude',
    '3CE-LIP-001-OAT',
    NULL,
    30.00,
    50,
    1,
    1
);

SET @VariantId_OAT = SCOPE_IDENTITY();

-- Insert images for variant OAT
INSERT INTO ProductVariantImage (VariantId, Url, AltText, IsPrimary, DisplayOrder)
VALUES
    -- Main image (Ảnh chính đại diện) - TODO: Quyết định ảnh nào là MAIN IMAGE (IsPrimary = 1)
    (@VariantId_OAT, @ImageBasePath + '/%2301%20OAT/3CE_L01_main.jpg.avif', '3CE Cashmere Hug Lipstick #01 OAT - Main', 1, 1),
    -- Product images
    (@VariantId_OAT, @ImageBasePath + '/%2301%20OAT/3CE_L01_lipstick1.jpg', '3CE Cashmere Hug Lipstick #01 OAT - Product View 1', 0, 2),
    (@VariantId_OAT, @ImageBasePath + '/%2301%20OAT/3CE_L01_lipstick2.jpg', '3CE Cashmere Hug Lipstick #01 OAT - Product View 2', 0, 3),
    (@VariantId_OAT, @ImageBasePath + '/%2301%20OAT/3CE_L01_swatch.jpg', '3CE Cashmere Hug Lipstick #01 OAT - Color Swatch', 0, 4),
    -- ⭐ VIRTUAL TRY-ON IMAGE - Ảnh này được dùng làm makeup_reference trong Virtual Try-On API
    (@VariantId_OAT, @ImageBasePath + '/%2301%20OAT/3CE_L01_model1.png', '3CE Cashmere Hug Lipstick #01 OAT - Model (Virtual Try-On)', 0, 5);

PRINT 'Variant #01 OAT inserted successfully. VariantId: ' + CAST(@VariantId_OAT AS NVARCHAR(10));

-- =============================================
-- Variant 2: #03 YOUR SIDE
-- =============================================
DECLARE @VariantId_YOURSIDE INT;

INSERT INTO ProductVariant (
    ProductId,
    VariantName,
    ColorName,
    ColorCode,
    ColorFamily,
    SKU,
    Price,
    CompareAtPrice,
    Stock,
    IsDefault,
    DisplayOrder
)
VALUES (
    @ProductId,
    '#03 YOUR SIDE',
    'YOUR SIDE',
    '#C89A7B',
    'Nude',
    '3CE-LIP-003-YOURSIDE',
    NULL,
    30.00,
    45,
    0,
    2
);

SET @VariantId_YOURSIDE = SCOPE_IDENTITY();

INSERT INTO ProductVariantImage (VariantId, Url, AltText, IsPrimary, DisplayOrder)
VALUES
    -- Main image - TODO: Quyết định ảnh nào là MAIN IMAGE (IsPrimary = 1)
    (@VariantId_YOURSIDE, @ImageBasePath + '/%2303%20YOUR%20SIDE/3CE_L03_main.jpg.avif', '3CE Cashmere Hug Lipstick #03 YOUR SIDE - Main', 1, 1),
    -- Product images
    (@VariantId_YOURSIDE, @ImageBasePath + '/%2303%20YOUR%20SIDE/3CE_L03_lipstick1.jpg', '3CE Cashmere Hug Lipstick #03 YOUR SIDE - Product View 1', 0, 2),
    (@VariantId_YOURSIDE, @ImageBasePath + '/%2303%20YOUR%20SIDE/3CE_L03_lipstick2.jpg', '3CE Cashmere Hug Lipstick #03 YOUR SIDE - Product View 2', 0, 3),
    (@VariantId_YOURSIDE, @ImageBasePath + '/%2303%20YOUR%20SIDE/3CE_L03_swatch.jpg.avif', '3CE Cashmere Hug Lipstick #03 YOUR SIDE - Color Swatch', 0, 4),
    -- ⭐ VIRTUAL TRY-ON IMAGE - Ảnh này được dùng làm makeup_reference trong Virtual Try-On API
    (@VariantId_YOURSIDE, @ImageBasePath + '/%2303%20YOUR%20SIDE/3CE_L03_model1.jpg', '3CE Cashmere Hug Lipstick #03 YOUR SIDE - Model (Virtual Try-On)', 0, 5);

PRINT 'Variant #03 YOUR SIDE inserted successfully. VariantId: ' + CAST(@VariantId_YOURSIDE AS NVARCHAR(10));

-- =============================================
-- Variant 3: #04 TEXT ME
-- =============================================
DECLARE @VariantId_TEXTME INT;

INSERT INTO ProductVariant (
    ProductId,
    VariantName,
    ColorName,
    ColorCode,
    ColorFamily,
    SKU,
    Price,
    CompareAtPrice,
    Stock,
    IsDefault,
    DisplayOrder
)
VALUES (
    @ProductId,
    '#04 TEXT ME',
    'TEXT ME',
    '#B87A6B',
    'Pink',
    '3CE-LIP-004-TEXTME',
    NULL,
    30.00,
    60,
    0,
    3
);

SET @VariantId_TEXTME = SCOPE_IDENTITY();

INSERT INTO ProductVariantImage (VariantId, Url, AltText, IsPrimary, IsMakeupReference, DisplayOrder)
VALUES
    -- Main image
    (@VariantId_TEXTME, @ImageBasePath + '/%2304%20TEXT%20ME/3CE_L04_main.jpg.avif', '3CE Cashmere Hug Lipstick #04 TEXT ME - Main', 1, 0, 1),
    -- Product images
    (@VariantId_TEXTME, @ImageBasePath + '/%2304%20TEXT%20ME/3CE_L04_lipstick1.jpg', '3CE Cashmere Hug Lipstick #04 TEXT ME - Product View 1', 0, 0, 2),
    (@VariantId_TEXTME, @ImageBasePath + '/%2304%20TEXT%20ME/3CE_L04_lipstick2.jpg', '3CE Cashmere Hug Lipstick #04 TEXT ME - Product View 2', 0, 0, 3),
    (@VariantId_TEXTME, @ImageBasePath + '/%2304%20TEXT%20ME/3CE_L04_swatch.webp', '3CE Cashmere Hug Lipstick #04 TEXT ME - Color Swatch', 0, 0, 4),
    -- ⭐ VIRTUAL TRY-ON IMAGE - Ảnh này được dùng làm makeup_reference trong Virtual Try-On API (IsMakeupReference = 1)
    (@VariantId_TEXTME, @ImageBasePath + '/%2304%20TEXT%20ME/3CE_L04_model1.jpeg', '3CE Cashmere Hug Lipstick #04 TEXT ME - Model (Virtual Try-On)', 0, 1, 5);

PRINT 'Variant #04 TEXT ME inserted successfully. VariantId: ' + CAST(@VariantId_TEXTME AS NVARCHAR(10));

-- =============================================
-- Variant 4: #06 BUDDY
-- =============================================
DECLARE @VariantId_BUDDY INT;

INSERT INTO ProductVariant (
    ProductId,
    VariantName,
    ColorName,
    ColorCode,
    ColorFamily,
    SKU,
    Price,
    CompareAtPrice,
    Stock,
    IsDefault,
    DisplayOrder
)
VALUES (
    @ProductId,
    '#06 BUDDY',
    'BUDDY',
    '#A67C52',
    'Brown',
    '3CE-LIP-006-BUDDY',
    NULL,
    30.00,
    40,
    0,
    4
);

SET @VariantId_BUDDY = SCOPE_IDENTITY();

INSERT INTO ProductVariantImage (VariantId, Url, AltText, IsPrimary, IsMakeupReference, DisplayOrder)
VALUES
    -- Main image
    (@VariantId_BUDDY, @ImageBasePath + '/%2306%20BUDDY/3CE_L06_main.jpg', '3CE Cashmere Hug Lipstick #06 BUDDY - Main', 1, 0, 1),
    -- Product images
    (@VariantId_BUDDY, @ImageBasePath + '/%2306%20BUDDY/3CE_L06_lipstick1.jpg', '3CE Cashmere Hug Lipstick #06 BUDDY - Product View 1', 0, 0, 2),
    (@VariantId_BUDDY, @ImageBasePath + '/%2306%20BUDDY/3CE_L06_lipstick2.jpg', '3CE Cashmere Hug Lipstick #06 BUDDY - Product View 2', 0, 0, 3),
    (@VariantId_BUDDY, @ImageBasePath + '/%2306%20BUDDY/3CE_L06_swatch.jpg.avif', '3CE Cashmere Hug Lipstick #06 BUDDY - Color Swatch', 0, 0, 4),
    -- ⭐ VIRTUAL TRY-ON IMAGE - Ảnh này được dùng làm makeup_reference trong Virtual Try-On API (IsMakeupReference = 1)
    (@VariantId_BUDDY, @ImageBasePath + '/%2306%20BUDDY/3CE_L06_model2.jpg.webp', '3CE Cashmere Hug Lipstick #06 BUDDY - Model (Virtual Try-On)', 0, 1, 5);

PRINT 'Variant #06 BUDDY inserted successfully. VariantId: ' + CAST(@VariantId_BUDDY AS NVARCHAR(10));

-- =============================================
-- Variant 5: #08 HUSH RED
-- =============================================
DECLARE @VariantId_HUSHRED INT;

INSERT INTO ProductVariant (
    ProductId,
    VariantName,
    ColorName,
    ColorCode,
    ColorFamily,
    SKU,
    Price,
    CompareAtPrice,
    Stock,
    IsDefault,
    DisplayOrder
)
VALUES (
    @ProductId,
    '#08 HUSH RED',
    'HUSH RED',
    '#C84257',
    'Red',
    '3CE-LIP-008-HUSHRED',
    NULL,
    30.00,
    55,
    0,
    5
);

SET @VariantId_HUSHRED = SCOPE_IDENTITY();

INSERT INTO ProductVariantImage (VariantId, Url, AltText, IsPrimary, IsMakeupReference, DisplayOrder)
VALUES
    -- Main image
    (@VariantId_HUSHRED, @ImageBasePath + '/%2308%20HUSH%20RED/3CE_L08_main.jpg.avif', '3CE Cashmere Hug Lipstick #08 HUSH RED - Main', 1, 0, 1),
    -- Product images
    (@VariantId_HUSHRED, @ImageBasePath + '/%2308%20HUSH%20RED/3CE_L08_lipstick1.jpg', '3CE Cashmere Hug Lipstick #08 HUSH RED - Product View 1', 0, 0, 2),
    (@VariantId_HUSHRED, @ImageBasePath + '/%2308%20HUSH%20RED/3CE_L08_lipstick2.webp', '3CE Cashmere Hug Lipstick #08 HUSH RED - Product View 2', 0, 0, 3),
    (@VariantId_HUSHRED, @ImageBasePath + '/%2308%20HUSH%20RED/3CE_L08_swatch.jpeg', '3CE Cashmere Hug Lipstick #08 HUSH RED - Color Swatch', 0, 0, 4),
    -- ⭐ VIRTUAL TRY-ON IMAGE - Ảnh này được dùng làm makeup_reference trong Virtual Try-On API (IsMakeupReference = 1)
    (@VariantId_HUSHRED, @ImageBasePath + '/%2308%20HUSH%20RED/3CE_L08_model1.png', '3CE Cashmere Hug Lipstick #08 HUSH RED - Model (Virtual Try-On)', 0, 1, 5);

PRINT 'Variant #08 HUSH RED inserted successfully. VariantId: ' + CAST(@VariantId_HUSHRED AS NVARCHAR(10));
GO

-- =============================================
-- Step 5: Insert Product Images (Optional - Ảnh chung không phụ thuộc màu)
-- =============================================
-- TODO: Nếu có ảnh chung của sản phẩm (packaging, demo, etc.), thêm vào đây
-- INSERT INTO ProductImage (ProductId, Url, AltText, ImageType, DisplayOrder)
-- VALUES
--     (@ProductId, @ImageBasePath + '/packaging.jpg', '3CE Cashmere Hug Lipstick - Packaging', 'gallery', 1),
--     (@ProductId, @ImageBasePath + '/demo.jpg', '3CE Cashmere Hug Lipstick - Demo', 'demo', 2);

COMMIT TRANSACTION;

PRINT '';
PRINT '=============================================';
PRINT '✅ 3CE Cashmere Hug Lipstick data inserted successfully!';
PRINT '=============================================';
PRINT 'Product: 3CE Cashmere Hug Lipstick';
PRINT 'Variants: 5 colors (#01 OAT, #03 YOUR SIDE, #04 TEXT ME, #06 BUDDY, #08 HUSH RED)';
PRINT '';
PRINT '⭐ Virtual Try-On Images:';
PRINT '   - #01 OAT: 3CE_L01_model1.png';
PRINT '   - #03 YOUR SIDE: 3CE_L03_model1.jpg';
PRINT '   - #04 TEXT ME: 3CE_L04_model1.jpeg';
PRINT '   - #06 BUDDY: 3CE_L06_model2.jpg';
PRINT '   - #08 HUSH RED: 3CE_L08_model1.png';
PRINT '';
PRINT '📁 Image Path: /public/images/products/3CE-Cashmere-Hug-Lipstick/';
PRINT '';
PRINT '✅ Script completed!';
PRINT '=============================================';

SET NOCOUNT OFF;
GO
