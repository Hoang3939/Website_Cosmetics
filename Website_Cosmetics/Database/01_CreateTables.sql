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
    UID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);

-- Create Users table
CREATE TABLE Users (
    UID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
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
    UID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserUID UNIQUEIDENTIFIER NOT NULL,
    RoleUID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserUID) REFERENCES Users(UID) ON DELETE CASCADE,
    FOREIGN KEY (RoleUID) REFERENCES Roles(UID) ON DELETE CASCADE,
    UNIQUE(UserUID, RoleUID)
);

-- Create PasswordResetTokens table
CREATE TABLE PasswordResetTokens (
    UID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserUID UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt DATETIME2(7) NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserUID) REFERENCES Users(UID) ON DELETE CASCADE
);

-- Create EmailConfirmationTokens table
CREATE TABLE EmailConfirmationTokens (
    UID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserUID UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt DATETIME2(7) NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserUID) REFERENCES Users(UID) ON DELETE CASCADE
);

-- =============================================
-- Permission System Tables
-- =============================================

-- Create Permissions table
CREATE TABLE Permissions (
    UID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PermissionName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    Category NVARCHAR(50), -- Admin, Staff, User
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);

-- Create RolePermissions table
CREATE TABLE RolePermissions (
    UID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RoleUID UNIQUEIDENTIFIER NOT NULL,
    PermissionUID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (RoleUID) REFERENCES Roles(UID) ON DELETE CASCADE,
    FOREIGN KEY (PermissionUID) REFERENCES Permissions(UID) ON DELETE CASCADE,
    UNIQUE(RoleUID, PermissionUID)
);

-- Create UserPermissions table
CREATE TABLE UserPermissions (
    UID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserUID UNIQUEIDENTIFIER NOT NULL,
    PermissionUID UNIQUEIDENTIFIER NOT NULL,
    GrantedBy UNIQUEIDENTIFIER, -- Admin nào cấp quyền
    GrantedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    ExpiresAt DATETIME2(7), -- Quyền có hạn sử dụng
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserUID) REFERENCES Users(UID) ON DELETE CASCADE,
    FOREIGN KEY (PermissionUID) REFERENCES Permissions(UID) ON DELETE CASCADE,
    FOREIGN KEY (GrantedBy) REFERENCES Users(UID),
    UNIQUE(UserUID, PermissionUID)
);

-- =============================================
-- Product & Business Tables
-- =============================================

-- Create Brand table
CREATE TABLE Brand (
    BrandId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Country NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- Create Category table
CREATE TABLE Category (
    CategoryId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- Create Product table
CREATE TABLE Product (
    ProductId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Slug NVARCHAR(200) NULL,
    Price DECIMAL(12,2) NOT NULL DEFAULT 0,
    Description NVARCHAR(MAX) NULL,
    Ingredients NVARCHAR(MAX) NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    BrandId UNIQUEIDENTIFIER,
    CategoryId UNIQUEIDENTIFIER,
    CONSTRAINT FK_Product_Brand FOREIGN KEY (BrandId)
        REFERENCES Brand(BrandId)
        ON DELETE SET NULL,
    CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryId)
        REFERENCES Category(CategoryId)
        ON DELETE SET NULL
);

-- Create ProductImage table
CREATE TABLE ProductImage (
    ProductImageId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Url NVARCHAR(255) NOT NULL,
    IsCover BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_ProductImage_Product FOREIGN KEY (ProductId)
        REFERENCES Product(ProductId)
        ON DELETE CASCADE
);

-- =============================================
-- Indexes for Performance Optimization
-- =============================================

-- Authentication indexes
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_PasswordResetTokens_Token ON PasswordResetTokens(Token);
CREATE INDEX IX_PasswordResetTokens_UserUID ON PasswordResetTokens(UserUID);
CREATE INDEX IX_EmailConfirmationTokens_Token ON EmailConfirmationTokens(Token);
CREATE INDEX IX_EmailConfirmationTokens_UserUID ON EmailConfirmationTokens(UserUID);
CREATE INDEX IX_UserRoles_UserUID ON UserRoles(UserUID);
CREATE INDEX IX_UserRoles_RoleUID ON UserRoles(RoleUID);

-- Permission indexes
CREATE INDEX IX_Permissions_PermissionName ON Permissions(PermissionName);
CREATE INDEX IX_Permissions_Category ON Permissions(Category);
CREATE INDEX IX_RolePermissions_RoleUID ON RolePermissions(RoleUID);
CREATE INDEX IX_RolePermissions_PermissionUID ON RolePermissions(PermissionUID);
CREATE INDEX IX_UserPermissions_UserUID ON UserPermissions(UserUID);
CREATE INDEX IX_UserPermissions_PermissionUID ON UserPermissions(PermissionUID);
CREATE INDEX IX_UserPermissions_GrantedBy ON UserPermissions(GrantedBy);

-- Product indexes
CREATE UNIQUE INDEX IX_Brand_Name ON Brand(Name);
CREATE UNIQUE INDEX IX_Category_Name ON Category(Name);
CREATE INDEX IX_Product_Name ON Product(Name);
CREATE UNIQUE INDEX IX_Product_Slug ON Product(Slug);
CREATE INDEX IX_Product_BrandId ON Product(BrandId);
CREATE INDEX IX_Product_CategoryId ON Product(CategoryId);
CREATE INDEX IX_ProductImage_ProductId ON ProductImage(ProductId);

PRINT 'All tables created successfully!';
PRINT 'Database: WebsiteCosmetic';
PRINT 'Total tables: 10';
PRINT 'Ready for data insertion.';
