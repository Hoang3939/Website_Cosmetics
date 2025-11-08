-- =====================================================
-- Script: Chuyển sang dùng ảnh MÔI làm makeup reference
-- Chạy sau khi đã test xong với ảnh người mẫu full mặt
-- =====================================================

USE WebsiteCosmetic;
GO

DECLARE @ProductId UNIQUEIDENTIFIER = '031EA73B-C58C-4E9C-81A9-A7C0C0996C76';

PRINT '=====================================================';
PRINT '🔄 CHUYỂN SANG TEST ẢNH MÔI';
PRINT '=====================================================';

-- Tắt makeup reference của ảnh người mẫu
UPDATE dbo.ProductImage 
SET IsMakeupReference = 0 
WHERE ProductId = @ProductId 
  AND Url LIKE '%nguoi-mau%';

PRINT '✓ Đã tắt makeup reference của ảnh người mẫu';

-- Bật makeup reference cho ảnh môi
UPDATE dbo.ProductImage 
SET IsMakeupReference = 1 
WHERE ProductId = @ProductId 
  AND Url LIKE '%moi-mau%';

PRINT '✓ Đã bật makeup reference cho ảnh môi';

-- Kiểm tra kết quả
PRINT '';
PRINT '📋 KẾT QUẢ:';
SELECT 
    Url,
    CASE WHEN IsCover = 1 THEN 'Yes' ELSE 'No' END AS [Ảnh bìa?],
    CASE WHEN IsMakeupReference = 1 THEN 'Yes ⭐' ELSE 'No' END AS [Makeup Reference?]
FROM dbo.ProductImage
WHERE ProductId = @ProductId
ORDER BY IsMakeupReference DESC, IsCover DESC;

PRINT '';
PRINT '✅ Bây giờ có thể test với ảnh môi!';
PRINT '=====================================================';

GO

