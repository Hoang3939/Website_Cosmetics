-- =====================================================
-- Script: 01_CreateTables.sql
-- Mô tả: Tạo toàn bộ cấu trúc database cho Website Cosmetics
-- Tác giả: AI Assistant
-- Ngày tạo: 2025-11-03
-- =====================================================

USE master;
GO

-- Xóa database cũ nếu tồn tại (CẢNH BÁO: Mất toàn bộ dữ liệu!)
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'WebsiteCosmetic')
BEGIN
    ALTER DATABASE WebsiteCosmetic SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE WebsiteCosmetic;
    PRINT '🗑️  Đã xóa database cũ: WebsiteCosmetic';
END
GO

-- Tạo database mới
CREATE DATABASE WebsiteCosmetic;
GO
PRINT '✅ Đã tạo database: WebsiteCosmetic';
GO

USE WebsiteCosmetic;
GO

-- =====================================================
-- BẢNG 1: Brand (Thương hiệu)
-- =====================================================
CREATE TABLE dbo.Brand (
    BrandId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL UNIQUE,
    Country NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);
PRINT '✅ Đã tạo bảng: Brand';
GO

-- =====================================================
-- BẢNG 2: Category (Danh mục sản phẩm)
-- =====================================================
CREATE TABLE dbo.Category (
    CategoryId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);
PRINT '✅ Đã tạo bảng: Category';
GO

-- =====================================================
-- BẢNG 3: Product (Sản phẩm)
-- =====================================================
CREATE TABLE dbo.Product (
    ProductId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Slug NVARCHAR(200) NULL UNIQUE,
    Price DECIMAL(12, 2) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Ingredients NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    BrandId UNIQUEIDENTIFIER NULL,
    CategoryId UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT FK_Product_Brand FOREIGN KEY (BrandId) 
        REFERENCES dbo.Brand(BrandId) ON DELETE SET NULL,
    CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryId) 
        REFERENCES dbo.Category(CategoryId) ON DELETE SET NULL
);

CREATE NONCLUSTERED INDEX IX_Product_Name ON dbo.Product(Name);
CREATE NONCLUSTERED INDEX IX_Product_Slug ON dbo.Product(Slug);
PRINT '✅ Đã tạo bảng: Product';
GO

-- =====================================================
-- BẢNG 4: ProductImage (Ảnh sản phẩm)
-- =====================================================
CREATE TABLE dbo.ProductImage (
    ProductImageId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    IsCover BIT NOT NULL DEFAULT 0,
    IsMakeupReference BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_ProductImage_Product FOREIGN KEY (ProductId) 
        REFERENCES dbo.Product(ProductId) ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX IX_ProductImage_ProductId ON dbo.ProductImage(ProductId);
CREATE NONCLUSTERED INDEX IX_ProductImage_IsCover ON dbo.ProductImage(IsCover);
CREATE NONCLUSTERED INDEX IX_ProductImage_IsMakeupReference ON dbo.ProductImage(IsMakeupReference);
PRINT '✅ Đã tạo bảng: ProductImage';
GO

-- =====================================================
-- BẢNG 5: User (Người dùng)
-- =====================================================
CREATE TABLE dbo.[User] (
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(50) NULL,
    LastName NVARCHAR(50) NULL,
    PhoneNumber NVARCHAR(11) NULL,
    IsEmailConfirmed BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);

CREATE NONCLUSTERED INDEX IX_User_Username ON dbo.[User](Username);
CREATE NONCLUSTERED INDEX IX_User_Email ON dbo.[User](Email);
PRINT '✅ Đã tạo bảng: User';
GO

-- =====================================================
-- BẢNG 6: Role (Vai trò)
-- =====================================================
CREATE TABLE dbo.Role (
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);

CREATE NONCLUSTERED INDEX IX_Role_RoleName ON dbo.Role(RoleName);
PRINT '✅ Đã tạo bảng: Role';
GO

-- =====================================================
-- BẢNG 7: UserRole (Phân quyền người dùng)
-- =====================================================
CREATE TABLE dbo.UserRole (
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserUID UNIQUEIDENTIFIER NOT NULL,
    RoleUID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_UserRole_User FOREIGN KEY (UserUID) 
        REFERENCES dbo.[User](UID) ON DELETE CASCADE,
    CONSTRAINT FK_UserRole_Role FOREIGN KEY (RoleUID) 
        REFERENCES dbo.Role(UID) ON DELETE CASCADE,
    CONSTRAINT UQ_UserRole UNIQUE (UserUID, RoleUID)
);

CREATE NONCLUSTERED INDEX IX_UserRole_UserUID ON dbo.UserRole(UserUID);
CREATE NONCLUSTERED INDEX IX_UserRole_RoleUID ON dbo.UserRole(RoleUID);
PRINT '✅ Đã tạo bảng: UserRole';
GO

-- =====================================================
-- BẢNG 8: Permission (Quyền hạn)
-- =====================================================
CREATE TABLE dbo.Permission (
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    PermissionName NVARCHAR(100) NOT NULL UNIQUE,
    Category NVARCHAR(50) NULL,
    Description NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);

CREATE NONCLUSTERED INDEX IX_Permission_PermissionName ON dbo.Permission(PermissionName);
CREATE NONCLUSTERED INDEX IX_Permission_Category ON dbo.Permission(Category);
PRINT '✅ Đã tạo bảng: Permission';
GO

-- =====================================================
-- BẢNG 9: RolePermission (Quyền của vai trò)
-- =====================================================
CREATE TABLE dbo.RolePermission (
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    RoleUID UNIQUEIDENTIFIER NOT NULL,
    PermissionUID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_RolePermission_Role FOREIGN KEY (RoleUID) 
        REFERENCES dbo.Role(UID) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermission_Permission FOREIGN KEY (PermissionUID) 
        REFERENCES dbo.Permission(UID) ON DELETE CASCADE,
    CONSTRAINT UQ_RolePermission UNIQUE (RoleUID, PermissionUID)
);

CREATE NONCLUSTERED INDEX IX_RolePermission_RoleUID ON dbo.RolePermission(RoleUID);
CREATE NONCLUSTERED INDEX IX_RolePermission_PermissionUID ON dbo.RolePermission(PermissionUID);
PRINT '✅ Đã tạo bảng: RolePermission';
GO

-- =====================================================
-- BẢNG 10: UserPermission (Quyền cá nhân của người dùng)
-- =====================================================
CREATE TABLE dbo.UserPermission (
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserUID UNIQUEIDENTIFIER NOT NULL,
    PermissionUID UNIQUEIDENTIFIER NOT NULL,
    GrantedBy UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_UserPermission_User FOREIGN KEY (UserUID) 
        REFERENCES dbo.[User](UID) ON DELETE CASCADE,
    CONSTRAINT FK_UserPermission_Permission FOREIGN KEY (PermissionUID) 
        REFERENCES dbo.Permission(UID) ON DELETE CASCADE,
    CONSTRAINT FK_UserPermission_GrantedBy FOREIGN KEY (GrantedBy) 
        REFERENCES dbo.[User](UID) ON DELETE NO ACTION,
    CONSTRAINT UQ_UserPermission UNIQUE (UserUID, PermissionUID)
);

CREATE NONCLUSTERED INDEX IX_UserPermission_UserUID ON dbo.UserPermission(UserUID);
CREATE NONCLUSTERED INDEX IX_UserPermission_PermissionUID ON dbo.UserPermission(PermissionUID);
CREATE NONCLUSTERED INDEX IX_UserPermission_GrantedBy ON dbo.UserPermission(GrantedBy);
PRINT '✅ Đã tạo bảng: UserPermission';
GO

-- =====================================================
-- BẢNG 11: PasswordResetToken (Token đặt lại mật khẩu)
-- =====================================================
CREATE TABLE dbo.PasswordResetToken (
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserUID UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt DATETIME2(7) NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_PasswordResetToken_User FOREIGN KEY (UserUID) 
        REFERENCES dbo.[User](UID) ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX IX_PasswordResetToken_UserUID ON dbo.PasswordResetToken(UserUID);
CREATE NONCLUSTERED INDEX IX_PasswordResetToken_Token ON dbo.PasswordResetToken(Token);
PRINT '✅ Đã tạo bảng: PasswordResetToken';
GO

-- =====================================================
-- BẢNG 12: EmailConfirmationToken (Token xác nhận email)
-- =====================================================
CREATE TABLE dbo.EmailConfirmationToken (
    UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserUID UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt DATETIME2(7) NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_EmailConfirmationToken_User FOREIGN KEY (UserUID) 
        REFERENCES dbo.[User](UID) ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX IX_EmailConfirmationToken_UserUID ON dbo.EmailConfirmationToken(UserUID);
CREATE NONCLUSTERED INDEX IX_EmailConfirmationToken_Token ON dbo.EmailConfirmationToken(Token);
PRINT '✅ Đã tạo bảng: EmailConfirmationToken';
GO

-- =====================================================
-- HOÀN TẤT
-- =====================================================
PRINT '';
PRINT '=====================================================';
PRINT '🎉 ĐÃ TẠO XONG TẤT CẢ CÁC BẢNG!';
PRINT '=====================================================';
PRINT 'Database: WebsiteCosmetic';
PRINT 'Tổng số bảng: 12';
PRINT '';
PRINT '📋 Danh sách bảng:';
PRINT '  1. Brand';
PRINT '  2. Category';
PRINT '  3. Product';
PRINT '  4. ProductImage (có IsMakeupReference)';
PRINT '  5. User';
PRINT '  6. Role';
PRINT '  7. UserRole';
PRINT '  8. Permission';
PRINT '  9. RolePermission';
PRINT ' 10. UserPermission';
PRINT ' 11. PasswordResetToken';
PRINT ' 12. EmailConfirmationToken';
PRINT '';
PRINT '📝 Bước tiếp theo: Chạy script 02_InsertData.sql';
PRINT '=====================================================';
GO

