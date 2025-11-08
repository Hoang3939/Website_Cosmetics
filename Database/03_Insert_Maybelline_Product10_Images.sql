-- =====================================================
-- Script: Thêm ảnh makeup reference cho sản phẩm Maybelline SuperStay Matte Ink
-- Product ID từ query: Row 10
-- =====================================================

USE WebsiteCosmetic;
GO

-- ProductId của sản phẩm Maybelline SuperStay Matte Ink (row 10 trong kết quả query)
DECLARE @ProductId UNIQUEIDENTIFIER = '031EA73B-C58C-4E9C-81A9-A7C0C0996C76';

-- Kiểm tra sản phẩm có tồn tại không
IF NOT EXISTS (SELECT 1 FROM dbo.Product WHERE ProductId = @ProductId)
BEGIN
    PRINT '❌ Không tìm thấy sản phẩm với ProductId: ' + CAST(@ProductId AS VARCHAR(50));
    PRINT '⚠️  Hãy kiểm tra lại ProductId trong bảng Product!';
    RETURN;
END

PRINT '✅ Tìm thấy sản phẩm!';
SELECT ProductId, Name FROM dbo.Product WHERE ProductId = @ProductId;

-- =====================================================
-- Xóa các ảnh makeup reference cũ (nếu có) để tránh trùng lặp
-- =====================================================
PRINT '';
PRINT '🗑️  Xóa các makeup reference cũ (nếu có)...';

UPDATE dbo.ProductImage 
SET IsMakeupReference = 0 
WHERE ProductId = @ProductId AND IsMakeupReference = 1;

-- =====================================================
-- Thêm 3 ảnh mới
-- =====================================================
PRINT '';
PRINT '📸 Đang thêm ảnh mới...';

-- 1. Ảnh cây son (Cover image - KHÔNG phải makeup reference)
INSERT INTO dbo.ProductImage (ProductImageId, ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(NEWID(), @ProductId, '/public/images/products/cay-son-superstay-135.jpg', 1, 0, GETDATE());
PRINT '   ✓ Đã thêm: cây son (Cover)';

-- 2. Ảnh người mẫu full mặt (Makeup Reference) ⭐ TEST TRƯỚC
INSERT INTO dbo.ProductImage (ProductImageId, ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(NEWID(), @ProductId, '/public/images/products/nguoi-mau-superstay-135.jpg', 0, 1, GETDATE());
PRINT '   ✓ Đã thêm: người mẫu full mặt (Makeup Reference) ⭐';

-- 3. Ảnh môi mẫu (CHƯA dùng makeup reference - test sau)
INSERT INTO dbo.ProductImage (ProductImageId, ProductId, Url, IsCover, IsMakeupReference, CreatedAt)
VALUES 
(NEWID(), @ProductId, '/public/images/products/moi-mau-superstay-135.jpg', 0, 0, GETDATE());
PRINT '   ✓ Đã thêm: môi mẫu (chưa set makeup reference - test sau)';

-- =====================================================
-- Kiểm tra kết quả
-- =====================================================
PRINT '';
PRINT '=====================================================';
PRINT '📋 KẾT QUẢ:';
PRINT '=====================================================';

SELECT 
    ProductImageId,
    Url,
    CASE WHEN IsCover = 1 THEN 'Yes' ELSE 'No' END AS [Ảnh bìa?],
    CASE WHEN IsMakeupReference = 1 THEN 'Yes ⭐' ELSE 'No' END AS [Makeup Reference?],
    FORMAT(CreatedAt, 'yyyy-MM-dd HH:mm') AS [Thời gian tạo]
FROM dbo.ProductImage
WHERE ProductId = @ProductId
ORDER BY IsCover DESC, IsMakeupReference DESC, CreatedAt DESC;

-- =====================================================
-- Hướng dẫn đổi tên file
-- =====================================================
PRINT '';
PRINT '=====================================================';
PRINT '📝 HƯỚNG DẪN ĐỔI TÊN FILE:';
PRINT '=====================================================';
PRINT 'Trong folder: D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\wwwroot\public\images\products\';
PRINT '';
PRINT '1. Đổi tên file:';
PRINT '   "cây son Super Stay Matte Ink Lipstick #135 - Globetrotter.jpg"';
PRINT '   → "cay-son-superstay-135.jpg"';
PRINT '';
PRINT '2. Đổi tên file:';
PRINT '   "ảnh người mẫu dùng son Super Stay Matte Ink Lipstick #135 - Globetrotter.jpg"';
PRINT '   → "nguoi-mau-superstay-135.jpg"';
PRINT '';
PRINT '3. Đổi tên file:';
PRINT '   "ảnh môi mẫu Super Stay Matte Ink Lipstick #135 - Globetrotter.jpg"';
PRINT '   → "moi-mau-superstay-135.jpg"';
PRINT '';
PRINT '⚠️  SAU KHI ĐỔI TÊN XONG, CHẠY LẠI SCRIPT NÀY!';
PRINT '=====================================================';

GO

