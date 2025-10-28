-- =============================================
-- Website Cosmetics Database - Data Insertion Script
-- =============================================
-- This script inserts sample data for the Website Cosmetics application
-- Run this script AFTER running 01_CreateTables.sql

USE WebsiteCosmetic;
GO

SET QUOTED_IDENTIFIER ON;
GO

-- =============================================
-- Insert Roles Data
-- =============================================

-- Insert default roles
INSERT INTO Roles (UID, RoleName, Description, IsActive, CreatedAt, UpdatedAt)
VALUES 
(NEWID(), 'Admin', 'Administrator with full system access', 1, GETDATE(), GETDATE()),
(NEWID(), 'Staff', 'Staff member with limited admin access', 1, GETDATE(), GETDATE()),
(NEWID(), 'User', 'Regular user with basic access', 1, GETDATE(), GETDATE());

-- =============================================
-- Insert Permissions Data
-- =============================================

-- Insert basic permissions
INSERT INTO Permissions (UID, PermissionName, Description, Category, IsActive, CreatedAt, UpdatedAt)
VALUES 
(NEWID(), 'Admin.Dashboard.View', 'View admin dashboard', 'Admin', 1, GETDATE(), GETDATE()),
(NEWID(), 'Staff.Dashboard.View', 'View staff dashboard', 'Staff', 1, GETDATE(), GETDATE()),
(NEWID(), 'User.Profile.View', 'View user profile', 'User', 1, GETDATE(), GETDATE()),
(NEWID(), 'Product.Manage', 'Manage products', 'Admin', 1, GETDATE(), GETDATE()),
(NEWID(), 'Category.Manage', 'Manage categories', 'Admin', 1, GETDATE(), GETDATE()),
(NEWID(), 'User.Manage', 'Manage users', 'Admin', 1, GETDATE(), GETDATE());

-- =============================================
-- Insert Brand Data
-- =============================================

DECLARE @Brand1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Brand2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Brand3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Brand4 UNIQUEIDENTIFIER = NEWID();
DECLARE @Brand5 UNIQUEIDENTIFIER = NEWID();

INSERT INTO Brand (BrandId, Name, Country, IsActive, CreatedAt, UpdatedAt)
VALUES
(@Brand1, 'L''Oréal', 'France', 1, GETDATE(), GETDATE()),
(@Brand2, 'Maybelline', 'USA', 1, GETDATE(), GETDATE()),
(@Brand3, 'MAC Cosmetics', 'Canada', 1, GETDATE(), GETDATE()),
(@Brand4, 'NARS', 'France', 1, GETDATE(), GETDATE()),
(@Brand5, 'Fenty Beauty', 'USA', 1, GETDATE(), GETDATE());

-- =============================================
-- Insert Category Data
-- =============================================

DECLARE @Category1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Category2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Category3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Category4 UNIQUEIDENTIFIER = NEWID();
DECLARE @Category5 UNIQUEIDENTIFIER = NEWID();

INSERT INTO Category (CategoryId, Name, Description, IsActive, CreatedAt, UpdatedAt)
VALUES
(@Category1, 'Lipstick', 'Matte, glossy, and tinted lip products', 1, GETDATE(), GETDATE()),
(@Category2, 'Eyeshadow', 'Diverse eyeshadow palettes and singles', 1, GETDATE(), GETDATE()),
(@Category3, 'Blush', 'Powder and cream blush products', 1, GETDATE(), GETDATE()),
(@Category4, 'Foundation', 'Foundation for all skin types', 1, GETDATE(), GETDATE()),
(@Category5, 'Setting Powder', 'Oil-control and finishing powders', 1, GETDATE(), GETDATE());

-- =============================================
-- Insert Product Data
-- =============================================

DECLARE @Product1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Product2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Product3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Product4 UNIQUEIDENTIFIER = NEWID();
DECLARE @Product5 UNIQUEIDENTIFIER = NEWID();

INSERT INTO Product (ProductId, Name, Slug, Price, Description, Ingredients, BrandId, CategoryId, IsActive, CreatedAt, UpdatedAt)
VALUES
(@Product1, 'Maybelline SuperStay Matte Ink', 'maybelline-superstay-matte-ink', 15.00,
 'Long-lasting matte lipstick with up to 16 hours wear.',
 'Isododecane, Dimethicone, Silica.', @Brand2, @Category1, 1, GETDATE(), GETDATE()),

(@Product2, 'L''Oréal Paradise Enchanted Palette', 'loreal-paradise-enchanted', 18.00,
 '12-shade eyeshadow palette with warm tones, easy to blend and long-lasting.', 
 'Talc, Mica, Dimethicone.', @Brand1, @Category2, 1, GETDATE(), GETDATE()),

(@Product3, 'NARS Orgasm Blush', 'nars-orgasm-blush', 32.00,
 'Iconic peachy-pink blush with golden shimmer.', 
 'Talc, Nylon-12, Dimethicone.', @Brand4, @Category3, 1, GETDATE(), GETDATE()),

(@Product4, 'MAC Studio Fix Fluid SPF15', 'mac-studio-fix-fluid', 35.00,
 'Full coverage foundation with oil control and smooth finish.', 
 'Water, Cyclopentasiloxane, Talc.', @Brand3, @Category4, 1, GETDATE(), GETDATE()),

(@Product5, 'Fenty Beauty Pro Filt''r Powder', 'fenty-pro-filtr-powder', 36.00,
 'Soft matte longwear setting powder for a smooth finish.', 
 'Silica, Dimethicone, Mica.', @Brand5, @Category5, 1, GETDATE(), GETDATE());

-- =============================================
-- Insert Product Image Data
-- =============================================

INSERT INTO ProductImage (ProductImageId, ProductId, Url, IsCover, CreatedAt)
VALUES
(NEWID(), @Product1, 'https://example.com/images/maybelline-superstay.jpg', 1, GETDATE()),
(NEWID(), @Product2, 'https://example.com/images/loreal-paradise-palette.jpg', 1, GETDATE()),
(NEWID(), @Product3, 'https://example.com/images/nars-orgasm-blush.jpg', 1, GETDATE()),
(NEWID(), @Product4, 'https://example.com/images/mac-studio-fix.jpg', 1, GETDATE()),
(NEWID(), @Product5, 'https://example.com/images/fenty-pro-filtr.jpg', 1, GETDATE());

-- =============================================
-- Create Sample Admin Account
-- =============================================

-- Create admin user (Password: Admin123!)
DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT UID FROM Roles WHERE RoleName = 'Admin');

INSERT INTO Users (UID, Username, Email, PasswordHash, FirstName, LastName, IsEmailConfirmed, IsActive, CreatedAt, UpdatedAt)
VALUES (@AdminUserId, 'admin', 'admin@cosmetics.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', 'User', 1, 1, GETDATE(), GETDATE());

-- Assign admin role
INSERT INTO UserRoles (UID, UserUID, RoleUID, CreatedAt, UpdatedAt)
VALUES (NEWID(), @AdminUserId, @AdminRoleId, GETDATE(), GETDATE());

-- =============================================
-- Create Sample Staff Account
-- =============================================

-- Create staff user (Password: Staff123!)
DECLARE @StaffUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @StaffRoleId UNIQUEIDENTIFIER = (SELECT UID FROM Roles WHERE RoleName = 'Staff');

INSERT INTO Users (UID, Username, Email, PasswordHash, FirstName, LastName, IsEmailConfirmed, IsActive, CreatedAt, UpdatedAt)
VALUES (@StaffUserId, 'staff', 'staff@cosmetics.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Staff', 'User', 1, 1, GETDATE(), GETDATE());

-- Assign staff role
INSERT INTO UserRoles (UID, UserUID, RoleUID, CreatedAt, UpdatedAt)
VALUES (NEWID(), @StaffUserId, @StaffRoleId, GETDATE(), GETDATE());

-- =============================================
-- Assign Role Permissions
-- =============================================

-- Admin permissions
DECLARE @AdminDashboardPermission UNIQUEIDENTIFIER = (SELECT UID FROM Permissions WHERE PermissionName = 'Admin.Dashboard.View');
DECLARE @ProductManagePermission UNIQUEIDENTIFIER = (SELECT UID FROM Permissions WHERE PermissionName = 'Product.Manage');
DECLARE @CategoryManagePermission UNIQUEIDENTIFIER = (SELECT UID FROM Permissions WHERE PermissionName = 'Category.Manage');
DECLARE @UserManagePermission UNIQUEIDENTIFIER = (SELECT UID FROM Permissions WHERE PermissionName = 'User.Manage');

-- Staff permissions
DECLARE @StaffDashboardPermission UNIQUEIDENTIFIER = (SELECT UID FROM Permissions WHERE PermissionName = 'Staff.Dashboard.View');

-- Assign permissions to Admin role
INSERT INTO RolePermissions (UID, RoleUID, PermissionUID, CreatedAt, UpdatedAt)
VALUES 
(NEWID(), @AdminRoleId, @AdminDashboardPermission, GETDATE(), GETDATE()),
(NEWID(), @AdminRoleId, @ProductManagePermission, GETDATE(), GETDATE()),
(NEWID(), @AdminRoleId, @CategoryManagePermission, GETDATE(), GETDATE()),
(NEWID(), @AdminRoleId, @UserManagePermission, GETDATE(), GETDATE());

-- Assign permissions to Staff role
INSERT INTO RolePermissions (UID, RoleUID, PermissionUID, CreatedAt, UpdatedAt)
VALUES 
(NEWID(), @StaffRoleId, @StaffDashboardPermission, GETDATE(), GETDATE()),
(NEWID(), @StaffRoleId, @AdminDashboardPermission, GETDATE(), GETDATE());

-- =============================================
-- Summary
-- =============================================

PRINT 'Data insertion completed successfully!';
PRINT '=============================================';
PRINT 'Summary:';
PRINT '- Roles: 3 (Admin, Staff, User)';
PRINT '- Permissions: 6';
PRINT '- Brands: 5';
PRINT '- Categories: 5';
PRINT '- Products: 5';
PRINT '- Product Images: 5';
PRINT '- Users: 2 (admin, staff)';
PRINT '- User Roles: 2';
PRINT '- Role Permissions: 6';
PRINT '=============================================';
PRINT 'Sample Accounts:';
PRINT 'Admin - Username: admin, Password: Admin123!';
PRINT 'Staff - Username: staff, Password: Staff123!';
PRINT '=============================================';
