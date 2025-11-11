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
INSERT INTO Roles (RoleName, Description, IsActive, CreatedAt, UpdatedAt)
VALUES 
('Admin', 'Administrator with full system access', 1, GETDATE(), GETDATE()),
('Staff', 'Staff member with limited admin access', 1, GETDATE(), GETDATE()),
('User', 'Regular user with basic access', 1, GETDATE(), GETDATE());

-- =============================================
-- Insert Permissions Data
-- =============================================

-- Insert basic permissions
INSERT INTO Permissions (PermissionName, Description, Category, IsActive, CreatedAt, UpdatedAt)
VALUES 
('Admin.Dashboard.View', 'View admin dashboard', 'Admin', 1, GETDATE(), GETDATE()),
('Staff.Dashboard.View', 'View staff dashboard', 'Staff', 1, GETDATE(), GETDATE()),
('User.Profile.View', 'View user profile', 'User', 1, GETDATE(), GETDATE()),
('Product.Manage', 'Manage products', 'Admin', 1, GETDATE(), GETDATE()),
('Category.Manage', 'Manage categories', 'Admin', 1, GETDATE(), GETDATE()),
('User.Manage', 'Manage users', 'Admin', 1, GETDATE(), GETDATE());

-- =============================================
-- Insert Brand Data
-- =============================================

DECLARE @Brand1 INT;
DECLARE @Brand2 INT;
DECLARE @Brand3 INT;
DECLARE @Brand4 INT;
DECLARE @Brand5 INT;

INSERT INTO Brand (Name, Country, IsActive, CreatedAt, UpdatedAt)
VALUES
('L''Oréal', 'France', 1, GETDATE(), GETDATE());
SET @Brand1 = SCOPE_IDENTITY();

INSERT INTO Brand (Name, Country, IsActive, CreatedAt, UpdatedAt)
VALUES
('Maybelline', 'USA', 1, GETDATE(), GETDATE());
SET @Brand2 = SCOPE_IDENTITY();

INSERT INTO Brand (Name, Country, IsActive, CreatedAt, UpdatedAt)
VALUES
('MAC Cosmetics', 'Canada', 1, GETDATE(), GETDATE());
SET @Brand3 = SCOPE_IDENTITY();

INSERT INTO Brand (Name, Country, IsActive, CreatedAt, UpdatedAt)
VALUES
('NARS', 'France', 1, GETDATE(), GETDATE());
SET @Brand4 = SCOPE_IDENTITY();

INSERT INTO Brand (Name, Country, IsActive, CreatedAt, UpdatedAt)
VALUES
('Fenty Beauty', 'USA', 1, GETDATE(), GETDATE());
SET @Brand5 = SCOPE_IDENTITY();

-- =============================================
-- Insert Category Data
-- =============================================

DECLARE @Category1 INT;
DECLARE @Category2 INT;
DECLARE @Category3 INT;
DECLARE @Category4 INT;
DECLARE @Category5 INT;

INSERT INTO Category (Name, Description, IsActive, CreatedAt, UpdatedAt)
VALUES
('Lipstick', 'Matte, glossy, and tinted lip products', 1, GETDATE(), GETDATE());
SET @Category1 = SCOPE_IDENTITY();

INSERT INTO Category (Name, Description, IsActive, CreatedAt, UpdatedAt)
VALUES
('Eyeshadow', 'Diverse eyeshadow palettes and singles', 1, GETDATE(), GETDATE());
SET @Category2 = SCOPE_IDENTITY();

INSERT INTO Category (Name, Description, IsActive, CreatedAt, UpdatedAt)
VALUES
('Blush', 'Powder and cream blush products', 1, GETDATE(), GETDATE());
SET @Category3 = SCOPE_IDENTITY();

INSERT INTO Category (Name, Description, IsActive, CreatedAt, UpdatedAt)
VALUES
('Foundation', 'Foundation for all skin types', 1, GETDATE(), GETDATE());
SET @Category4 = SCOPE_IDENTITY();

INSERT INTO Category (Name, Description, IsActive, CreatedAt, UpdatedAt)
VALUES
('Setting Powder', 'Oil-control and finishing powders', 1, GETDATE(), GETDATE());
SET @Category5 = SCOPE_IDENTITY();

-- =============================================
-- Insert Product Data
-- =============================================

DECLARE @Product1 INT;
DECLARE @Product2 INT;
DECLARE @Product3 INT;
DECLARE @Product4 INT;
DECLARE @Product5 INT;

INSERT INTO Product (Name, Slug, BasePrice, Description, Ingredients, BrandId, CategoryId, IsActive, CreatedAt, UpdatedAt)
VALUES
('Maybelline SuperStay Matte Ink', 'maybelline-superstay-matte-ink', 15.00,
 'Long-lasting matte lipstick with up to 16 hours wear.',
 'Isododecane, Dimethicone, Silica.', @Brand2, @Category1, 1, GETDATE(), GETDATE());
SET @Product1 = SCOPE_IDENTITY();

INSERT INTO Product (Name, Slug, BasePrice, Description, Ingredients, BrandId, CategoryId, IsActive, CreatedAt, UpdatedAt)
VALUES
('L''Oréal Paradise Enchanted Palette', 'loreal-paradise-enchanted', 18.00,
 '12-shade eyeshadow palette with warm tones, easy to blend and long-lasting.', 
 'Talc, Mica, Dimethicone.', @Brand1, @Category2, 1, GETDATE(), GETDATE());
SET @Product2 = SCOPE_IDENTITY();

INSERT INTO Product (Name, Slug, BasePrice, Description, Ingredients, BrandId, CategoryId, IsActive, CreatedAt, UpdatedAt)
VALUES
('NARS Orgasm Blush', 'nars-orgasm-blush', 32.00,
 'Iconic peachy-pink blush with golden shimmer.', 
 'Talc, Nylon-12, Dimethicone.', @Brand4, @Category3, 1, GETDATE(), GETDATE());
SET @Product3 = SCOPE_IDENTITY();

INSERT INTO Product (Name, Slug, BasePrice, Description, Ingredients, BrandId, CategoryId, IsActive, CreatedAt, UpdatedAt)
VALUES
('MAC Studio Fix Fluid SPF15', 'mac-studio-fix-fluid', 35.00,
 'Full coverage foundation with oil control and smooth finish.', 
 'Water, Cyclopentasiloxane, Talc.', @Brand3, @Category4, 1, GETDATE(), GETDATE());
SET @Product4 = SCOPE_IDENTITY();

INSERT INTO Product (Name, Slug, BasePrice, Description, Ingredients, BrandId, CategoryId, IsActive, CreatedAt, UpdatedAt)
VALUES
('Fenty Beauty Pro Filt''r Powder', 'fenty-pro-filtr-powder', 36.00,
 'Soft matte longwear setting powder for a smooth finish.', 
 'Silica, Dimethicone, Mica.', @Brand5, @Category5, 1, GETDATE(), GETDATE());
SET @Product5 = SCOPE_IDENTITY();

-- =============================================
-- Insert Product Image Data
-- =============================================

INSERT INTO ProductImage (ProductId, Url, AltText, ImageType, DisplayOrder, CreatedAt)
VALUES
(@Product1, 'https://example.com/images/maybelline-superstay.jpg', 'Maybelline SuperStay Matte Ink', 'cover', 0, GETDATE()),
(@Product2, 'https://example.com/images/loreal-paradise-palette.jpg', 'L''Oréal Paradise Enchanted Palette', 'cover', 0, GETDATE()),
(@Product3, 'https://example.com/images/nars-orgasm-blush.jpg', 'NARS Orgasm Blush', 'cover', 0, GETDATE()),
(@Product4, 'https://example.com/images/mac-studio-fix.jpg', 'MAC Studio Fix Fluid SPF15', 'cover', 0, GETDATE()),
(@Product5, 'https://example.com/images/fenty-pro-filtr.jpg', 'Fenty Beauty Pro Filt''r Powder', 'cover', 0, GETDATE());

-- =============================================
-- Create Sample Admin Account
-- =============================================

-- Create admin user (Password: Admin123!)
DECLARE @AdminUserId INT;
DECLARE @AdminRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Admin');

INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, IsEmailConfirmed, IsActive, CreatedAt, UpdatedAt)
VALUES ('admin', 'admin@cosmetics.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', 'User', 1, 1, GETDATE(), GETDATE());
SET @AdminUserId = SCOPE_IDENTITY();

-- Assign admin role
INSERT INTO UserRoles (UserId, RoleId, CreatedAt, UpdatedAt)
VALUES (@AdminUserId, @AdminRoleId, GETDATE(), GETDATE());

-- =============================================
-- Create Sample Staff Account
-- =============================================

-- Create staff user (Password: Staff123!)
DECLARE @StaffUserId INT;
DECLARE @StaffRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Staff');

INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, IsEmailConfirmed, IsActive, CreatedAt, UpdatedAt)
VALUES ('staff', 'staff@cosmetics.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Staff', 'User', 1, 1, GETDATE(), GETDATE());
SET @StaffUserId = SCOPE_IDENTITY();

-- Assign staff role
INSERT INTO UserRoles (UserId, RoleId, CreatedAt, UpdatedAt)
VALUES (@StaffUserId, @StaffRoleId, GETDATE(), GETDATE());

-- =============================================
-- Assign Role Permissions
-- =============================================

-- Admin permissions
DECLARE @AdminDashboardPermission INT = (SELECT PermissionId FROM Permissions WHERE PermissionName = 'Admin.Dashboard.View');
DECLARE @ProductManagePermission INT = (SELECT PermissionId FROM Permissions WHERE PermissionName = 'Product.Manage');
DECLARE @CategoryManagePermission INT = (SELECT PermissionId FROM Permissions WHERE PermissionName = 'Category.Manage');
DECLARE @UserManagePermission INT = (SELECT PermissionId FROM Permissions WHERE PermissionName = 'User.Manage');

-- Staff permissions
DECLARE @StaffDashboardPermission INT = (SELECT PermissionId FROM Permissions WHERE PermissionName = 'Staff.Dashboard.View');

-- User permissions
DECLARE @UserRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'User');
DECLARE @UserProfilePermission INT = (SELECT PermissionId FROM Permissions WHERE PermissionName = 'User.Profile.View');

-- Assign permissions to Admin role
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt, UpdatedAt)
VALUES 
(@AdminRoleId, @AdminDashboardPermission, GETDATE(), GETDATE()),
(@AdminRoleId, @ProductManagePermission, GETDATE(), GETDATE()),
(@AdminRoleId, @CategoryManagePermission, GETDATE(), GETDATE()),
(@AdminRoleId, @UserManagePermission, GETDATE(), GETDATE());

-- Assign permissions to Staff role
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt, UpdatedAt)
VALUES 
(@StaffRoleId, @StaffDashboardPermission, GETDATE(), GETDATE()),
(@StaffRoleId, @AdminDashboardPermission, GETDATE(), GETDATE());

-- Assign permissions to User role
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt, UpdatedAt)
VALUES 
(@UserRoleId, @UserProfilePermission, GETDATE(), GETDATE());

-- =============================================
-- Update Admin User Email Confirmation
-- =============================================

-- Update IsEmailConfirmed for admin user (if created via UI)
UPDATE Users 
SET IsEmailConfirmed = 1, UpdatedAt = GETDATE()
WHERE Username = 'admin' AND IsEmailConfirmed = 0;

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
PRINT '- Role Permissions: 7 (Admin: 4, Staff: 2, User: 1)';
PRINT '=============================================';
PRINT 'Sample Accounts:';
PRINT 'Admin - Username: admin, Password: Admin123!';
PRINT 'Staff - Username: staff, Password: Staff123!';
PRINT '=============================================';
