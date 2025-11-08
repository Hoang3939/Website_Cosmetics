-- Script: Thêm cột IsMakeupReference vào bảng ProductImage
-- Mục đích: Đánh dấu ảnh nào sẽ được dùng làm mẫu makeup reference cho API PSGAN

USE WebsiteCosmetic;
GO

-- Kiểm tra nếu cột chưa tồn tại thì mới thêm
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ProductImage' 
    AND COLUMN_NAME = 'IsMakeupReference'
)
BEGIN
    ALTER TABLE dbo.ProductImage
    ADD IsMakeupReference BIT NOT NULL DEFAULT 0;
    
    PRINT 'Đã thêm cột IsMakeupReference vào bảng ProductImage';
END
ELSE
BEGIN
    PRINT 'Cột IsMakeupReference đã tồn tại';
END
GO

-- Thêm index để tìm kiếm nhanh ảnh makeup reference theo ProductId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductImage_MakeupReference')
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductImage_MakeupReference
    ON dbo.ProductImage(ProductId, IsMakeupReference)
    WHERE IsMakeupReference = 1;
    
    PRINT 'Đã tạo index IX_ProductImage_MakeupReference';
END
GO

-- Xem kết quả
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ProductImage'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Script hoàn tất!';
PRINT 'Lưu ý: Bạn cần cập nhật data để đánh dấu ảnh nào là makeup reference:';
PRINT 'UPDATE dbo.ProductImage SET IsMakeupReference = 1 WHERE ProductImageId = <id_cua_anh_mau>';
GO

