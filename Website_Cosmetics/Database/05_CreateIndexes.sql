-- =============================================
-- Website Cosmetics Database - Index Creation Script
-- =============================================
-- This script creates indexes to optimize query performance
-- Run this script after creating tables and inserting data

USE WebsiteCosmetic;
GO

-- =============================================
-- Product Indexes
-- =============================================

-- Index for filtering active products
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Product_IsActive' AND object_id = OBJECT_ID('Product'))
BEGIN
    CREATE INDEX IX_Product_IsActive ON Product(IsActive);
    PRINT 'Created index IX_Product_IsActive';
END
GO

-- Index for ordering by LikeCount
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Product_LikeCount' AND object_id = OBJECT_ID('Product'))
BEGIN
    CREATE INDEX IX_Product_LikeCount ON Product(LikeCount DESC);
    PRINT 'Created index IX_Product_LikeCount';
END
GO

-- Index for ordering by Rating
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Product_Rating' AND object_id = OBJECT_ID('Product'))
BEGIN
    CREATE INDEX IX_Product_Rating ON Product(Rating DESC);
    PRINT 'Created index IX_Product_Rating';
END
GO

-- Composite index for Most Loved query (IsActive, LikeCount, Rating)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Product_IsActive_LikeCount_Rating' AND object_id = OBJECT_ID('Product'))
BEGIN
    CREATE INDEX IX_Product_IsActive_LikeCount_Rating ON Product(IsActive, LikeCount DESC, Rating DESC);
    PRINT 'Created index IX_Product_IsActive_LikeCount_Rating';
END
GO

-- Index for UpdatedAt (for OnSale query)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Product_UpdatedAt' AND object_id = OBJECT_ID('Product'))
BEGIN
    CREATE INDEX IX_Product_UpdatedAt ON Product(UpdatedAt DESC);
    PRINT 'Created index IX_Product_UpdatedAt';
END
GO

-- Composite index for OnSale query (IsActive, UpdatedAt)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Product_IsActive_UpdatedAt' AND object_id = OBJECT_ID('Product'))
BEGIN
    CREATE INDEX IX_Product_IsActive_UpdatedAt ON Product(IsActive, UpdatedAt DESC);
    PRINT 'Created index IX_Product_IsActive_UpdatedAt';
END
GO

-- =============================================
-- ProductImage Indexes
-- =============================================

-- Index for filtering by ImageType (cover images)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductImage_ProductId_ImageType' AND object_id = OBJECT_ID('ProductImage'))
BEGIN
    CREATE INDEX IX_ProductImage_ProductId_ImageType ON ProductImage(ProductId, ImageType);
    PRINT 'Created index IX_ProductImage_ProductId_ImageType';
END
GO

-- Index for DisplayOrder
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductImage_DisplayOrder' AND object_id = OBJECT_ID('ProductImage'))
BEGIN
    CREATE INDEX IX_ProductImage_DisplayOrder ON ProductImage(ProductId, DisplayOrder);
    PRINT 'Created index IX_ProductImage_DisplayOrder';
END
GO

-- =============================================
-- ProductVariant Indexes
-- =============================================

-- Index for filtering active variants with sale prices
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductVariant_IsActive_CompareAtPrice_Price' AND object_id = OBJECT_ID('ProductVariant'))
BEGIN
    CREATE INDEX IX_ProductVariant_IsActive_CompareAtPrice_Price ON ProductVariant(IsActive, ProductId) INCLUDE (CompareAtPrice, Price);
    PRINT 'Created index IX_ProductVariant_IsActive_CompareAtPrice_Price';
END
GO

-- =============================================
-- Print completion message
-- =============================================
PRINT '=============================================';
PRINT 'Index creation completed!';
PRINT '=============================================';
GO

