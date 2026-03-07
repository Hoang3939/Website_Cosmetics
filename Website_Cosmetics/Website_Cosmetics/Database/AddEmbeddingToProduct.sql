-- Migration: Add Embedding column to Product table for RAG functionality
-- Run this script directly on your SQL Server database

IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Product]') 
    AND name = 'Embedding'
)
BEGIN
    ALTER TABLE [dbo].[Product]
    ADD [Embedding] NVARCHAR(MAX) NULL;
    
    PRINT '✅ Column Embedding added to Product table';
END
ELSE
BEGIN
    PRINT '⚠️ Column Embedding already exists in Product table';
END
GO


