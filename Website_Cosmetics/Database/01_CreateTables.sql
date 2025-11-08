-- =============================================
-- Website Cosmetics Database - Table Creation Script
-- =============================================
-- This script creates all necessary tables for the Website Cosmetics application
-- Run this script first before running the data insertion script

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WebsiteCosmetic')
BEGIN
    CREATE DATABASE WebsiteCosmetic;
END
GO

USE WebsiteCosmetic;
GO

SET QUOTED_IDENTIFIER ON;
GO

-- =============================================
-- Authentication & Authorization Tables
-- =============================================

-- Create Roles table
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);

-- Create Users table
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    PhoneNumber NVARCHAR(20),
    IsEmailConfirmed BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);

-- Create UserRoles table
CREATE TABLE UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE,
    UNIQUE(UserId, RoleId)
);

-- Create PasswordResetTokens table
CREATE TABLE PasswordResetTokens (
    TokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Token NVARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt DATETIME2(7) NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- Create EmailConfirmationTokens table
CREATE TABLE EmailConfirmationTokens (
    TokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Token NVARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt DATETIME2(7) NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- =============================================
-- Permission System Tables
-- =============================================

-- Create Permissions table
CREATE TABLE Permissions (
    PermissionId INT PRIMARY KEY IDENTITY(1,1),
    PermissionName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    Category NVARCHAR(50), -- Admin, Staff, User
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);

-- Create RolePermissions table
CREATE TABLE RolePermissions (
    RolePermissionId INT PRIMARY KEY IDENTITY(1,1),
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId) ON DELETE CASCADE,
    UNIQUE(RoleId, PermissionId)
);

-- Create UserPermissions table
CREATE TABLE UserPermissions (
    UserPermissionId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    PermissionId INT NOT NULL,
    GrantedBy INT, -- Admin nào cấp quyền
    GrantedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    ExpiresAt DATETIME2(7), -- Quyền có hạn sử dụng
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId) ON DELETE CASCADE,
    FOREIGN KEY (GrantedBy) REFERENCES Users(UserId),
    UNIQUE(UserId, PermissionId)
);

-- =============================================
-- Product & Business Tables
-- =============================================

-- Create Brand table
CREATE TABLE Brand (
    BrandId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Country NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- Create Category table
CREATE TABLE Category (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- Create Product table (Sản phẩm chính - VD: HERA Sensual Nude Lip Gloss)
CREATE TABLE Product (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Slug NVARCHAR(200) NULL,
    BasePrice DECIMAL(12,2) NOT NULL DEFAULT 0, -- Giá cơ bản
    Description NVARCHAR(MAX) NULL,
    Ingredients NVARCHAR(MAX) NULL,
    Rating DECIMAL(3,2) NULL, -- VD: 4.5
    ReviewCount INT DEFAULT 0,
    LikeCount INT DEFAULT 0,
    SPF NVARCHAR(50) NULL, -- VD: "SPF 25"
    Size NVARCHAR(50) NULL, -- VD: "0.14 oz / 4g"
    Finish NVARCHAR(50) NULL, -- VD: "Natural finish"
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    BrandId INT,
    CategoryId INT,
    CONSTRAINT FK_Product_Brand FOREIGN KEY (BrandId)
        REFERENCES Brand(BrandId)
        ON DELETE SET NULL,
    CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryId)
        REFERENCES Category(CategoryId)
        ON DELETE SET NULL
);

-- Create ProductVariant table (Biến thể của sản phẩm - Màu sắc, Size, hoặc kết hợp)
-- VD: 
--   Son môi: Color = "Midnight Lily", Size = NULL
--   Chai xịt: Color = NULL, Size = "100ml"
--   Phấn má: Color = "Rose Pink", Size = "10g"
--   Kem dưỡng: Color = NULL, Size = "50ml", Type = "For Oily Skin"
CREATE TABLE ProductVariant (
    VariantId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    
    -- Thông tin biến thể
    VariantName NVARCHAR(200) NOT NULL, -- Tên đầy đủ: "Midnight Lily", "100ml", "Rose Pink - 10g"
    
    -- Màu sắc (cho son, phấn, nail polish, etc.)
    ColorName NVARCHAR(100) NULL, -- VD: "Midnight Lily", "Rose Pink", "Coral Sunset"
    ColorCode NVARCHAR(50) NULL, -- Mã màu hex, VD: "#8B4F5E"
    ColorFamily NVARCHAR(50) NULL, -- Nhóm màu: "Red", "Pink", "Nude", "Brown"
    
    -- Kích thước/Dung tích (cho chai xịt, kem, serum, etc.)
    Size NVARCHAR(100) NULL, -- VD: "50ml", "100ml", "10g", "0.14 oz"
    SizeValue DECIMAL(10,2) NULL, -- Giá trị số: 50, 100, 10
    SizeUnit NVARCHAR(20) NULL, -- Đơn vị: "ml", "g", "oz", "fl oz"
    
    -- Loại/Type (cho sản phẩm skincare)
    VariantType NVARCHAR(100) NULL, -- VD: "For Oily Skin", "For Dry Skin", "Anti-Aging", "Brightening"
    
    -- Thông tin bán hàng
    SKU NVARCHAR(100) NULL, -- Mã SKU riêng cho từng variant
    Barcode NVARCHAR(100) NULL, -- Mã vạch
    Price DECIMAL(12,2) NULL, -- Giá riêng nếu khác BasePrice, NULL thì dùng BasePrice của Product
    CompareAtPrice DECIMAL(12,2) NULL, -- Giá gốc (để hiển thị giảm giá)
    Stock INT DEFAULT 0, -- Số lượng tồn kho
    LowStockThreshold INT DEFAULT 10, -- Ngưỡng cảnh báo hết hàng
    
    -- Trạng thái
    IsActive BIT DEFAULT 1,
    IsDefault BIT DEFAULT 0, -- Variant mặc định khi vào trang sản phẩm
    DisplayOrder INT DEFAULT 0, -- Thứ tự hiển thị
    
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT FK_ProductVariant_Product FOREIGN KEY (ProductId)
        REFERENCES Product(ProductId)
        ON DELETE CASCADE
);

-- Create ProductVariantImage table (Ảnh cho từng biến thể màu)
-- Khi người dùng click vào màu, sẽ hiển thị các ảnh của màu đó
CREATE TABLE ProductVariantImage (
    ImageId INT PRIMARY KEY IDENTITY(1,1),
    VariantId INT NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    AltText NVARCHAR(255) NULL,
    IsPrimary BIT DEFAULT 0, -- Ảnh chính của màu này
    IsMakeupReference BIT DEFAULT 0, -- Ảnh dùng cho Virtual Try-On (có khuôn mặt người mẫu)
    DisplayOrder INT DEFAULT 0, -- Thứ tự hiển thị ảnh
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_ProductVariantImage_Variant FOREIGN KEY (VariantId)
        REFERENCES ProductVariant(VariantId)
        ON DELETE CASCADE
);

-- Create ProductImage table (Ảnh chung của sản phẩm - không phụ thuộc màu)
-- VD: Ảnh packaging, ảnh demo, video review, etc.
CREATE TABLE ProductImage (
    ProductImageId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    AltText NVARCHAR(255) NULL,
    ImageType NVARCHAR(50) DEFAULT 'gallery', -- 'cover', 'gallery', 'demo', 'video'
    DisplayOrder INT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_ProductImage_Product FOREIGN KEY (ProductId)
        REFERENCES Product(ProductId)
        ON DELETE CASCADE
);

-- =============================================
-- Shopping Cart & Orders Tables
-- =============================================

-- Create ShoppingCart table
CREATE TABLE ShoppingCart (
    CartId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NULL, -- NULL nếu là guest
    SessionId NVARCHAR(255) NULL, -- Cho guest users
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- Create CartItem table
CREATE TABLE CartItem (
    CartItemId INT PRIMARY KEY IDENTITY(1,1),
    CartId INT NOT NULL,
    VariantId INT NOT NULL, -- Lưu variant cụ thể (màu sắc)
    Quantity INT NOT NULL DEFAULT 1,
    Price DECIMAL(12,2) NOT NULL, -- Giá tại thời điểm thêm vào giỏ
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CartId) REFERENCES ShoppingCart(CartId) ON DELETE CASCADE,
    FOREIGN KEY (VariantId) REFERENCES ProductVariant(VariantId) ON DELETE CASCADE
);

-- Create Orders table
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NULL,
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Processing, Shipped, Delivered, Cancelled
    SubTotal DECIMAL(12,2) NOT NULL,
    ShippingFee DECIMAL(12,2) DEFAULT 0,
    Discount DECIMAL(12,2) DEFAULT 0,
    Total DECIMAL(12,2) NOT NULL,
    ShippingAddress NVARCHAR(500),
    ShippingMethod NVARCHAR(100), -- Standard, Express, Same-Day
    PaymentMethod NVARCHAR(100),
    PaymentStatus NVARCHAR(50) DEFAULT 'Pending',
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE SET NULL
);

-- Create OrderItem table
CREATE TABLE OrderItem (
    OrderItemId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    VariantId INT NOT NULL,
    ProductName NVARCHAR(200) NOT NULL, -- Lưu tên sản phẩm tại thời điểm đặt
    ColorName NVARCHAR(100) NOT NULL, -- Lưu tên màu tại thời điểm đặt
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(12,2) NOT NULL,
    Subtotal DECIMAL(12,2) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
    FOREIGN KEY (VariantId) REFERENCES ProductVariant(VariantId)
);

-- =============================================
-- Reviews & Ratings Tables
-- =============================================

-- Create ProductReview table
CREATE TABLE ProductReview (
    ReviewId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Title NVARCHAR(200) NULL,
    Content NVARCHAR(MAX) NULL,
    IsVerifiedPurchase BIT DEFAULT 0,
    HelpfulCount INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (ProductId) REFERENCES Product(ProductId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- Create ProductLike table (Wishlist)
CREATE TABLE ProductLike (
    LikeId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (ProductId) REFERENCES Product(ProductId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    UNIQUE(ProductId, UserId)
);

-- =============================================
-- Indexes for Performance Optimization
-- =============================================

-- Authentication indexes
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_PasswordResetTokens_Token ON PasswordResetTokens(Token);
CREATE INDEX IX_PasswordResetTokens_UserId ON PasswordResetTokens(UserId);
CREATE INDEX IX_EmailConfirmationTokens_Token ON EmailConfirmationTokens(Token);
CREATE INDEX IX_EmailConfirmationTokens_UserId ON EmailConfirmationTokens(UserId);
CREATE INDEX IX_UserRoles_UserId ON UserRoles(UserId);
CREATE INDEX IX_UserRoles_RoleId ON UserRoles(RoleId);

-- Permission indexes
CREATE INDEX IX_Permissions_PermissionName ON Permissions(PermissionName);
CREATE INDEX IX_Permissions_Category ON Permissions(Category);
CREATE INDEX IX_RolePermissions_RoleId ON RolePermissions(RoleId);
CREATE INDEX IX_RolePermissions_PermissionId ON RolePermissions(PermissionId);
CREATE INDEX IX_UserPermissions_UserId ON UserPermissions(UserId);
CREATE INDEX IX_UserPermissions_PermissionId ON UserPermissions(PermissionId);
CREATE INDEX IX_UserPermissions_GrantedBy ON UserPermissions(GrantedBy);

-- Product indexes
CREATE UNIQUE INDEX IX_Brand_Name ON Brand(Name);
CREATE UNIQUE INDEX IX_Category_Name ON Category(Name);
CREATE INDEX IX_Product_Name ON Product(Name);
CREATE UNIQUE INDEX IX_Product_Slug ON Product(Slug);
CREATE INDEX IX_Product_BrandId ON Product(BrandId);
CREATE INDEX IX_Product_CategoryId ON Product(CategoryId);
CREATE INDEX IX_ProductVariant_ProductId ON ProductVariant(ProductId);
CREATE INDEX IX_ProductVariant_ColorName ON ProductVariant(ColorName);
CREATE INDEX IX_ProductVariant_Size ON ProductVariant(Size);
CREATE INDEX IX_ProductVariant_SKU ON ProductVariant(SKU);
CREATE INDEX IX_ProductVariantImage_VariantId ON ProductVariantImage(VariantId);
CREATE INDEX IX_ProductImage_ProductId ON ProductImage(ProductId);

-- Shopping indexes
CREATE INDEX IX_ShoppingCart_UserId ON ShoppingCart(UserId);
CREATE INDEX IX_ShoppingCart_SessionId ON ShoppingCart(SessionId);
CREATE INDEX IX_CartItem_CartId ON CartItem(CartId);
CREATE INDEX IX_CartItem_VariantId ON CartItem(VariantId);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_OrderNumber ON Orders(OrderNumber);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_OrderItem_OrderId ON OrderItem(OrderId);
CREATE INDEX IX_OrderItem_VariantId ON OrderItem(VariantId);

-- Review indexes
CREATE INDEX IX_ProductReview_ProductId ON ProductReview(ProductId);
CREATE INDEX IX_ProductReview_UserId ON ProductReview(UserId);
CREATE INDEX IX_ProductLike_ProductId ON ProductLike(ProductId);
CREATE INDEX IX_ProductLike_UserId ON ProductLike(UserId);

PRINT '=============================================';
PRINT 'All tables created successfully!';
PRINT '=============================================';
PRINT 'Database: WebsiteCosmetic';
PRINT 'Total tables: 23';
PRINT '';
PRINT 'Core Tables:';
PRINT '- Authentication: Users, Roles, UserRoles (7 tables)';
PRINT '- Permissions: Permissions, RolePermissions, UserPermissions (3 tables)';
PRINT '- Products: Brand, Category, Product, ProductVariant (4 tables)';
PRINT '- Images: ProductImage, ProductVariantImage (2 tables)';
PRINT '- Shopping: ShoppingCart, CartItem, Orders, OrderItem (4 tables)';
PRINT '- Reviews: ProductReview, ProductLike (2 tables)';
PRINT '';
PRINT 'Key Features:';
PRINT '- INT IDENTITY primary keys for stable IDs';
PRINT '- Product Variant system for color variations';
PRINT '- Separate images for each color variant';
PRINT '- Shopping cart and order management';
PRINT '- Review and wishlist functionality';
PRINT '';
PRINT 'Ready for data insertion!';
PRINT '=============================================';
