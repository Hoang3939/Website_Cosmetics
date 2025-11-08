-- =====================================================
-- Script: INSERT PRODUCT + 3 IMAGES
-- Mục đích: Thêm sản phẩm Maybelline SuperStay và 3 ảnh
-- Ngày: 2025-11-04
-- =====================================================

USE WebsiteCosmetic;
GO

PRINT '=====================================================';
PRINT '🚀 BẮT ĐẦU THÊM SẢN PHẨM VÀ ẢNH';
PRINT '=====================================================';
PRINT '';

-- =====================================================
-- BƯỚC 1: KIỂM TRA VÀ THÊM BRAND (nếu chưa có)
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Brand WHERE BrandId = 'A7B3C5D9-8E2F-4A1B-9C3D-5E6F7A8B9C0D')
BEGIN
    PRINT '📦 Đang thêm Brand: Maybelline...';
    INSERT INTO dbo.Brand (BrandId, Name, Country, IsActive, CreatedAt, UpdatedAt) VALUES
    ('A7B3C5D9-8E2F-4A1B-9C3D-5E6F7A8B9C0D', N'Maybelline', N'USA', 1, GETDATE(), GETDATE());
    PRINT '   ✓ Đã thêm Brand: Maybelline';
END
ELSE
    PRINT '   ℹ️  Brand Maybelline đã tồn tại';
GO

-- =====================================================
-- BƯỚC 2: KIỂM TRA VÀ THÊM CATEGORY (nếu chưa có)
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Category WHERE CategoryId = 'F8C2E4A6-3B1D-5F7A-9C0E-2B4D6F8A0C1E')
BEGIN
    PRINT '📂 Đang thêm Category: Lipstick...';
    INSERT INTO dbo.Category (CategoryId, Name, Description, IsActive, CreatedAt, UpdatedAt) VALUES
    ('F8C2E4A6-3B1D-5F7A-9C0E-2B4D6F8A0C1E', N'Lipstick', N'Son môi các loại', 1, GETDATE(), GETDATE());
    PRINT '   ✓ Đã thêm Category: Lipstick';
END
ELSE
    PRINT '   ℹ️  Category Lipstick đã tồn tại';
GO

-- =====================================================
-- BƯỚC 3: XÓA DỮ LIỆU CŨ (nếu có)
-- =====================================================
PRINT '🗑️  Xóa dữ liệu cũ (nếu có)...';

-- Xóa ảnh trước (vì có Foreign Key)
DELETE FROM dbo.ProductImage 
WHERE ProductId = '7ADE93DF-3D89-47FA-B490-58C6CA4C2706';
PRINT '   ✓ Đã xóa ảnh cũ';

-- Xóa product
DELETE FROM dbo.Product 
WHERE ProductId = '7ADE93DF-3D89-47FA-B490-58C6CA4C2706';
PRINT '   ✓ Đã xóa product cũ';
GO

-- =====================================================
-- BƯỚC 4: THÊM PRODUCT MỚI
-- =====================================================
PRINT '🛍️  Đang thêm Product: Maybelline SuperStay Matte Ink #135...';

INSERT INTO dbo.Product (
    ProductId, 
    Name, 
    Slug, 
    Price, 
    Description, 
    Ingredients, 
    IsActive, 
    BrandId, 
    CategoryId, 
    CreatedAt, 
    UpdatedAt
) VALUES (
    '7ADE93DF-3D89-47FA-B490-58C6CA4C2706',
    N'Maybelline SuperStay Matte Ink Lipstick #135 - Globetrotter',
    N'maybelline-superstay-matte-ink-135-globetrotter',
    8.99,
    N'Long-lasting liquid matte lipstick with up to 16 hours wear. Intense color that stays put without smudging or fading.',
    N'Dimethicone, Trimethylsiloxysilicate, Synthetic Wax, Isododecane, Dimethicone Crosspolymer, Silica',
    1,
    'A7B3C5D9-8E2F-4A1B-9C3D-5E6F7A8B9C0D', -- Maybelline Brand
    'F8C2E4A6-3B1D-5F7A-9C0E-2B4D6F8A0C1E', -- Lipstick Category
    GETDATE(),
    GETDATE()
);

PRINT '   ✓ Đã thêm Product: Maybelline SuperStay Matte Ink #135';
GO

-- =====================================================
-- BƯỚC 5: THÊM 3 ẢNH CHO PRODUCT
-- =====================================================
PRINT '🖼️  Đang thêm 3 ảnh cho Product...';

INSERT INTO dbo.ProductImage (ProductImageId, ProductId, Url, IsCover, IsMakeupReference, CreatedAt) VALUES
-- 1. Ảnh cây son (Cover image - ảnh đại diện)
('9F2E4A6C-8B1D-3F5A-7C9E-0B2D4F6A8C1D', 
 '7ADE93DF-3D89-47FA-B490-58C6CA4C2706', 
 '/public/images/products/cay-son-superstay-135.jpg', 
 1,  -- IsCover = 1 (ảnh đại diện)
 0,  -- IsMakeupReference = 0
 GETDATE()),

-- 2. ⭐ Ảnh người mẫu (ẢNH MẪU MAKEUP - dùng cho Virtual Try-On)
('A1C3E5B7-9D2F-4A6C-8E0B-1D3F5A7C9E2B', 
 '7ADE93DF-3D89-47FA-B490-58C6CA4C2706', 
 '/public/images/products/nguoi-mau-superstay-135.jpg', 
 0,  -- IsCover = 0
 1,  -- IsMakeupReference = 1 ⭐ ĐÂY LÀ ẢNH MẪU MAKEUP
 GETDATE()),

-- 3. Ảnh môi mẫu (ảnh chi tiết)
('B4D6F8A0-2C1E-5B7D-9F3A-6C8E0B2D4F3A', 
 '7ADE93DF-3D89-47FA-B490-58C6CA4C2706', 
 '/public/images/products/moi-mau-superstay-135.jpg', 
 0,  -- IsCover = 0
 0,  -- IsMakeupReference = 0
 GETDATE());

PRINT '   ✓ Đã thêm 3 ảnh:';
PRINT '     - cay-son-superstay-135.jpg (Cover)';
PRINT '     - nguoi-mau-superstay-135.jpg (⭐ Makeup Reference)';
PRINT '     - moi-mau-superstay-135.jpg';
GO

-- =====================================================
-- BƯỚC 6: KIỂM TRA KẾT QUẢ
-- =====================================================
PRINT '';
PRINT '=====================================================';
PRINT '✅ HOÀN TẤT! Kiểm tra kết quả:';
PRINT '=====================================================';
PRINT '';

-- Kiểm tra Product
SELECT 
    ProductId,
    Name,
    Price,
    CASE WHEN IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS Status
FROM dbo.Product 
WHERE ProductId = '7ADE93DF-3D89-47FA-B490-58C6CA4C2706';

PRINT '';

-- Kiểm tra Images
SELECT 
    pi.ProductImageId,
    pi.Url AS [Đường dẫn ảnh],
    CASE WHEN pi.IsCover = 1 THEN 'Có' ELSE 'Không' END AS [Ảnh đại diện?],
    CASE WHEN pi.IsMakeupReference = 1 THEN '⭐ CÓ' ELSE 'Không' END AS [Ảnh mẫu makeup?],
    pi.CreatedAt AS [Ngày tạo]
FROM dbo.ProductImage pi
WHERE pi.ProductId = '7ADE93DF-3D89-47FA-B490-58C6CA4C2706'
ORDER BY pi.IsCover DESC, pi.IsMakeupReference DESC;

PRINT '';
PRINT '🎉 THÀNH CÔNG!';
PRINT '';
PRINT '📝 THÔNG TIN:';
PRINT '  • ProductId: 7ADE93DF-3D89-47FA-B490-58C6CA4C2706';
PRINT '  • Product Name: Maybelline SuperStay Matte Ink #135 - Globetrotter';
PRINT '  • Số lượng ảnh: 3';
PRINT '  • Ảnh mẫu makeup: nguoi-mau-superstay-135.jpg';
PRINT '';
PRINT '🚀 BƯỚC TIẾP THEO:';
PRINT '  1. Khởi động API Server: python D:\PSGAN-master\api_server_v2.py';
PRINT '  2. Khởi động Website: dotnet run';
PRINT '  3. Truy cập: http://localhost:5210/Products/Details/7ADE93DF-3D89-47FA-B490-58C6CA4C2706';
PRINT '  4. Click "Virtual Try-On" để test!';
PRINT '=====================================================';
GO




