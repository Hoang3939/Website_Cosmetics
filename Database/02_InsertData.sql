-- =====================================================
-- Script: 02_InsertData.sql
-- Mô tả: Thêm dữ liệu mẫu với GUID CỐ ĐỊNH (không thay đổi khi chạy lại)
-- Tác giả: AI Assistant
-- Ngày tạo: 2025-11-03
-- =====================================================

USE WebsiteCosmetic;
GO

PRINT '=====================================================';
PRINT '🚀 BẮT ĐẦU THÊM DỮ LIỆU MẪU';
PRINT '=====================================================';
PRINT '';

-- =====================================================
-- PHẦN 1: THÊM THƯƠNG HIỆU (Brand)
-- =====================================================
PRINT '📦 Đang thêm thương hiệu...';

INSERT INTO dbo.Brand (BrandId, Name, Country, IsActive, CreatedAt, UpdatedAt) VALUES
('A7B3C5D9-8E2F-4A1B-9C3D-5E6F7A8B9C0D', N'Maybelline', N'USA', 1, GETDATE(), GETDATE()),
('B2D4E6F8-9A1C-3B5D-7E9F-1A2B3C4D5E6F', N'L''Oréal Paris', N'France', 1, GETDATE(), GETDATE()),
('C9E1F3A5-7B2D-4C6E-8A0F-2B3C4D5E6F7A', N'MAC Cosmetics', N'Canada', 1, GETDATE(), GETDATE()),
('D4F6A8C0-2E1B-5D7F-9A3C-5E6F7A8B9C0D', N'NYX Professional Makeup', N'USA', 1, GETDATE(), GETDATE()),
('E7A9B1C3-5D2F-4E6A-8C0B-1D2E3F4A5B6C', N'Revlon', N'USA', 1, GETDATE(), GETDATE());

PRINT '   ✓ Đã thêm 5 thương hiệu';
GO

-- =====================================================
-- PHẦN 2: THÊM DANH MỤC (Category)
-- =====================================================
PRINT '📂 Đang thêm danh mục...';

INSERT INTO dbo.Category (CategoryId, Name, Description, IsActive, CreatedAt, UpdatedAt) VALUES
('F8C2E4A6-3B1D-5F7A-9C0E-2B4D6F8A0C1E', N'Lipstick', N'Son môi các loại', 1, GETDATE(), GETDATE()),
('A1C3E5B7-9D2F-4A6C-8E0B-1D3F5A7C9E0B', N'Foundation', N'Kem nền', 1, GETDATE(), GETDATE()),
('B4D6F8A0-2C1E-5B7D-9F3A-6C8E0B2D4F6A', N'Mascara', N'Mascara làm dày mi', 1, GETDATE(), GETDATE()),
('C7E9A1B3-5D4F-6A8C-0E2B-7D9F1A3C5E7B', N'Eyeshadow', N'Phấn mắt', 1, GETDATE(), GETDATE()),
('D0F2A4C6-8E1B-7D9F-3A5C-8E0B2D4F6A8C', N'Blush', N'Má hồng', 1, GETDATE(), GETDATE());

PRINT '   ✓ Đã thêm 5 danh mục';
GO

-- =====================================================
-- PHẦN 3: THÊM SẢN PHẨM (Product)
-- =====================================================
PRINT '🛍️  Đang thêm sản phẩm...';

-- Sản phẩm 1: Maybelline SuperStay Matte Ink (PRODUCT CHÍNH ĐỂ TEST VIRTUAL TRY-ON)
INSERT INTO dbo.Product (ProductId, Name, Slug, Price, Description, Ingredients, IsActive, BrandId, CategoryId, CreatedAt, UpdatedAt) VALUES
('7ADE93DF-3D89-47FA-B490-58C6CA4C2706', 
 N'Maybelline SuperStay Matte Ink Lipstick #135 - Globetrotter',
 N'maybelline-superstay-matte-ink-135-globetrotter',
 8.99,
 N'Long-lasting liquid matte lipstick with up to 16 hours wear. Intense color that stays put without smudging or fading.',
 N'Dimethicone, Trimethylsiloxysilicate, Synthetic Wax, Isododecane, Dimethicone Crosspolymer, Silica',
 1,
 'A7B3C5D9-8E2F-4A1B-9C3D-5E6F7A8B9C0D', -- Maybelline
 'F8C2E4A6-3B1D-5F7A-9C0E-2B4D6F8A0C1E', -- Lipstick
 GETDATE(),
 GETDATE());

PRINT '   ✓ Đã thêm: Maybelline SuperStay Matte Ink #135';

-- Sản phẩm 2-10: Các sản phẩm khác
INSERT INTO dbo.Product (ProductId, Name, Slug, Price, Description, IsActive, BrandId, CategoryId, CreatedAt, UpdatedAt) VALUES
('E3F8A2C7-9B4D-6E1F-5A3C-8D0B2E4F7A9C', N'L''Oréal Paris Infallible Pro-Matte Foundation', N'loreal-infallible-pro-matte-foundation', 12.99, N'Long-wear matte foundation with full coverage.', 1, 'B2D4E6F8-9A1C-3B5D-7E9F-1A2B3C4D5E6F', 'A1C3E5B7-9D2F-4A6C-8E0B-1D3F5A7C9E0B', GETDATE(), GETDATE()),
('F7C9E1A3-5B2D-8F4A-6C0E-9B1D3F5A7C9E', N'MAC Ruby Woo Lipstick', N'mac-ruby-woo-lipstick', 19.00, N'Iconic matte red lipstick with retro finish.', 1, 'C9E1F3A5-7B2D-4C6E-8A0F-2B3C4D5E6F7A', 'F8C2E4A6-3B1D-5F7A-9C0E-2B4D6F8A0C1E', GETDATE(), GETDATE()),
('A2D4B6C8-0E1F-9A3C-5D7E-2B4F6A8C0D1E', N'NYX Professional Makeup Epic Ink Liner', N'nyx-epic-ink-liner', 8.50, N'Waterproof liquid eyeliner with precision tip.', 1, 'D4F6A8C0-2E1B-5D7F-9A3C-5E6F7A8B9C0D', 'C7E9A1B3-5D4F-6A8C-0E2B-7D9F1A3C5E7B', GETDATE(), GETDATE()),
('B5E7C9A1-3D2F-6B8D-0F4A-7C9E1B3D5F7A', N'Maybelline Lash Sensational Mascara', N'maybelline-lash-sensational-mascara', 9.99, N'Volumizing mascara for longer, fuller lashes.', 1, 'A7B3C5D9-8E2F-4A1B-9C3D-5E6F7A8B9C0D', 'B4D6F8A0-2C1E-5B7D-9F3A-6C8E0B2D4F6A', GETDATE(), GETDATE()),
('C8F0A2D4-6E1B-9C3F-5A7D-0B2E4F6A8C0D', N'Revlon ColorStay Overtime Lipcolor', N'revlon-colorstay-overtime-lipcolor', 10.99, N'Dual-ended lipstick with color and top coat.', 1, 'E7A9B1C3-5D2F-4E6A-8C0B-1D2E3F4A5B6C', 'F8C2E4A6-3B1D-5F7A-9C0E-2B4D6F8A0C1E', GETDATE(), GETDATE()),
('D1F3A5C7-9E2B-8D4F-6A0C-3B5D7F9A1C3E', N'L''Oréal Paris Voluminous Lash Paradise Mascara', N'loreal-voluminous-lash-paradise-mascara', 11.99, N'Volumizing and lengthening mascara with soft brush.', 1, 'B2D4E6F8-9A1C-3B5D-7E9F-1A2B3C4D5E6F', 'B4D6F8A0-2C1E-5B7D-9F3A-6C8E0B2D4F6A', GETDATE(), GETDATE()),
('E4A6C8B0-2D1F-7E9A-3C5B-6D8F0A2C4E6A', N'NYX Professional Makeup Soft Matte Lip Cream', N'nyx-soft-matte-lip-cream', 6.99, N'Liquid lip cream with velvety matte finish.', 1, 'D4F6A8C0-2E1B-5D7F-9A3C-5E6F7A8B9C0D', 'F8C2E4A6-3B1D-5F7A-9C0E-2B4D6F8A0C1E', GETDATE(), GETDATE()),
('F9B1C3D5-7E0A-6F8B-2D4A-9C1E3F5A7C9B', N'MAC Studio Fix Powder Plus Foundation', N'mac-studio-fix-powder-foundation', 33.00, N'Powder foundation with medium to full coverage.', 1, 'C9E1F3A5-7B2D-4C6E-8A0F-2B3C4D5E6F7A', 'A1C3E5B7-9D2F-4A6C-8E0B-1D3F5A7C9E0B', GETDATE(), GETDATE()),
('A0C2E4F6-8B1D-5A7C-9E3F-2B4D6F8A0C2E', N'Maybelline Fit Me Matte + Poreless Foundation', N'maybelline-fit-me-matte-poreless-foundation', 7.99, N'Lightweight foundation that mattifies and refines pores.', 1, 'A7B3C5D9-8E2F-4A1B-9C3D-5E6F7A8B9C0D', 'A1C3E5B7-9D2F-4A6C-8E0B-1D3F5A7C9E0B', GETDATE(), GETDATE());

PRINT '   ✓ Đã thêm 9 sản phẩm nữa (tổng: 10 sản phẩm)';
GO

-- =====================================================
-- PHẦN 4: THÊM ẢNH SẢN PHẨM (ProductImage)
-- =====================================================
PRINT '🖼️  Đang thêm ảnh sản phẩm...';

-- ⭐ ẢNH CHO SẢN PHẨM MAYBELLINE SUPERSTAY MATTE INK #135 (Product Test)
-- ProductId: 7ADE93DF-3D89-47FA-B490-58C6CA4C2706
INSERT INTO dbo.ProductImage (ProductImageId, ProductId, Url, IsCover, IsMakeupReference, CreatedAt) VALUES
-- 1. Ảnh cây son (Cover image)
('9F2E4A6C-8B1D-3F5A-7C9E-0B2D4F6A8C1D', '7ADE93DF-3D89-47FA-B490-58C6CA4C2706', '/public/images/products/cay-son-superstay-135.jpg', 1, 0, GETDATE()),
-- 2. Ảnh người mẫu full mặt (MAKEUP REFERENCE - dùng để test Virtual Try-On) ⭐
('A1C3E5B7-9D2F-4A6C-8E0B-1D3F5A7C9E2B', '7ADE93DF-3D89-47FA-B490-58C6CA4C2706', '/public/images/products/nguoi-mau-superstay-135.jpg', 0, 1, GETDATE()),
-- 3. Ảnh môi mẫu (có thể dùng làm reference sau)
('B4D6F8A0-2C1E-5B7D-9F3A-6C8E0B2D4F3A', '7ADE93DF-3D89-47FA-B490-58C6CA4C2706', '/public/images/products/moi-mau-superstay-135.jpg', 0, 0, GETDATE());

PRINT '   ✓ Đã thêm 3 ảnh cho Maybelline SuperStay Matte Ink #135';
PRINT '   ⭐ Ảnh makeup reference: nguoi-mau-superstay-135.jpg';

-- Ảnh cho các sản phẩm khác (placeholder)
INSERT INTO dbo.ProductImage (ProductImageId, ProductId, Url, IsCover, IsMakeupReference, CreatedAt) VALUES
('C7E9A1B3-5D4F-6A8C-0E2B-7D9F1A3C5E4B', 'E3F8A2C7-9B4D-6E1F-5A3C-8D0B2E4F7A9C', '/public/images/products/placeholder-foundation.jpg', 1, 0, GETDATE()),
('D0F2A4C6-8E1B-7D9F-3A5C-8E0B2D4F6A5C', 'F7C9E1A3-5B2D-8F4A-6C0E-9B1D3F5A7C9E', '/public/images/products/placeholder-lipstick.jpg', 1, 0, GETDATE()),
('E3A5C7B9-1D0F-8E2A-4C6B-9D1F3A5C7E6D', 'A2D4B6C8-0E1F-9A3C-5D7E-2B4F6A8C0D1E', '/public/images/products/placeholder-eyeliner.jpg', 1, 0, GETDATE()),
('F6B8D0C2-4E1A-9F3B-7D5C-0E2A4C6B8D7E', 'B5E7C9A1-3D2F-6B8D-0F4A-7C9E1B3D5F7A', '/public/images/products/placeholder-mascara.jpg', 1, 0, GETDATE()),
('A9C1E3D5-7F0B-2A4C-6E8D-1F3A5C7E9B8F', 'C8F0A2D4-6E1B-9C3F-5A7D-0B2E4F6A8C0D', '/public/images/products/placeholder-lipstick.jpg', 1, 0, GETDATE()),
('B2D4F6A8-0C1E-5B7D-9F3A-6C8E0B2D4F9A', 'D1F3A5C7-9E2B-8D4F-6A0C-3B5D7F9A1C3E', '/public/images/products/placeholder-mascara.jpg', 1, 0, GETDATE()),
('C5E7A9B1-3D2F-8C0E-4A6B-7D9F1C3E5A0B', 'E4A6C8B0-2D1F-7E9A-3C5B-6D8F0A2C4E6A', '/public/images/products/placeholder-lipstick.jpg', 1, 0, GETDATE()),
('D8F0B2C4-6E1A-9D3F-7B5C-0E2A4F6B8D1C', 'F9B1C3D5-7E0A-6F8B-2D4A-9C1E3F5A7C9B', '/public/images/products/placeholder-foundation.jpg', 1, 0, GETDATE()),
('E1A3C5D7-9F2B-0E4A-8C6D-1F3B5D7F9A2D', 'A0C2E4F6-8B1D-5A7C-9E3F-2B4D6F8A0C2E', '/public/images/products/placeholder-foundation.jpg', 1, 0, GETDATE());

PRINT '   ✓ Đã thêm ảnh placeholder cho 9 sản phẩm khác';
GO

-- =====================================================
-- HOÀN TẤT
-- =====================================================
PRINT '';
PRINT '=====================================================';
PRINT '🎉 ĐÃ THÊM XONG TẤT CẢ DỮ LIỆU MẪU!';
PRINT '=====================================================';
PRINT '';
PRINT '📊 TỔNG KẾT DỮ LIỆU:';
PRINT '  • Brands: 5';
PRINT '  • Categories: 5';
PRINT '  • Products: 10';
PRINT '  • Product Images: 12 (3 cho Maybelline SuperStay + 9 placeholder)';
PRINT '';
PRINT '⚠️  LƯU Ý: Chưa có dữ liệu User/Role/Permission';
PRINT '           Bạn cần tạo user qua chức năng đăng ký của website';
PRINT '           để có password hash đúng chuẩn!';
PRINT '';
PRINT '⭐ SẢN PHẨM TEST VIRTUAL TRY-ON:';
PRINT '  ProductId: 7ADE93DF-3D89-47FA-B490-58C6CA4C2706';
PRINT '  Name: Maybelline SuperStay Matte Ink #135 - Globetrotter';
PRINT '  URL: /Products/Details/7ADE93DF-3D89-47FA-B490-58C6CA4C2706';
PRINT '  Makeup Reference Image: /public/images/products/nguoi-mau-superstay-135.jpg';
PRINT '';
PRINT '📝 LƯU Ý QUAN TRỌNG:';
PRINT '  ✅ TẤT CẢ GUID ĐÃ CỐ ĐỊNH - Không thay đổi khi chạy lại script!';
PRINT '  ✅ GUID trông ngẫu nhiên nhưng vẫn cố định (không dùng NEWID())';
PRINT '  ✅ Đảm bảo file ảnh đã được đổi tên đúng và copy vào:';
PRINT '     D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\wwwroot\public\images\products\';
PRINT '  ✅ Kiểm tra 3 file ảnh:';
PRINT '     - cay-son-superstay-135.jpg';
PRINT '     - nguoi-mau-superstay-135.jpg';
PRINT '     - moi-mau-superstay-135.jpg';
PRINT '';
PRINT '🚀 BƯỚC TIẾP THEO:';
PRINT '  1. Tạo user admin qua chức năng đăng ký của website';
PRINT '  2. Khởi động lại website ASP.NET (dotnet run)';
PRINT '  3. Truy cập: http://localhost:5210/Products/Details/7ADE93DF-3D89-47FA-B490-58C6CA4C2706';
PRINT '  4. Click nút "Virtual Try-On" để test!';
PRINT '=====================================================';
GO

