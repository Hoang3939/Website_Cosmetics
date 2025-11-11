-- =============================================
-- Fix Admin User Script
-- =============================================
-- This script fixes admin user issues:
-- 1. Sets IsEmailConfirmed = 1
-- 2. Ensures Admin role is assigned
-- 3. Shows current user status
--
-- Run this script to fix admin user login issues

USE WebsiteCosmetic;
GO

SET QUOTED_IDENTIFIER ON;
GO

PRINT '=============================================';
PRINT 'Fixing Admin User';
PRINT '=============================================';
PRINT '';

-- Check and fix admin user
DECLARE @AdminUserId INT;
DECLARE @AdminRoleId INT;
DECLARE @AdminUsername NVARCHAR(50) = 'admin'; -- Change this if your admin username is different

-- Get admin user
SELECT @AdminUserId = UserId 
FROM Users 
WHERE Username = @AdminUsername OR Email LIKE '%admin%';

IF @AdminUserId IS NULL
BEGIN
    PRINT '❌ ERROR: Admin user not found!';
    PRINT 'Please create admin user first or update @AdminUsername variable.';
    PRINT '';
    PRINT 'Current users in database:';
    SELECT UserId, Username, Email, IsEmailConfirmed, IsActive 
    FROM Users;
    RETURN;
END

-- Get Admin role
SELECT @AdminRoleId = RoleId 
FROM Roles 
WHERE RoleName = 'Admin';

IF @AdminRoleId IS NULL
BEGIN
    PRINT '❌ ERROR: Admin role not found!';
    PRINT 'Please run 02_InsertData.sql first to create roles.';
    RETURN;
END

-- Show current status
PRINT 'Current Admin User Status:';
SELECT 
    UserId,
    Username,
    Email,
    IsEmailConfirmed,
    IsActive,
    CreatedAt,
    UpdatedAt
FROM Users
WHERE UserId = @AdminUserId;
PRINT '';

-- Show current roles
PRINT 'Current Roles for Admin User:';
SELECT 
    r.RoleId,
    r.RoleName,
    r.Description,
    r.IsActive
FROM UserRoles ur
INNER JOIN Roles r ON ur.RoleId = r.RoleId
WHERE ur.UserId = @AdminUserId;
PRINT '';

-- Fix 1: Set IsEmailConfirmed = 1
IF EXISTS (SELECT 1 FROM Users WHERE UserId = @AdminUserId AND IsEmailConfirmed = 0)
BEGIN
    UPDATE Users 
    SET IsEmailConfirmed = 1, UpdatedAt = GETDATE()
    WHERE UserId = @AdminUserId;
    PRINT '✅ Fixed: Set IsEmailConfirmed = 1 for admin user';
END
ELSE
BEGIN
    PRINT 'ℹ️  IsEmailConfirmed is already set to 1';
END
PRINT '';

-- Fix 2: Assign Admin role if not assigned
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, CreatedAt, UpdatedAt)
    VALUES (@AdminUserId, @AdminRoleId, GETDATE(), GETDATE());
    PRINT '✅ Fixed: Assigned Admin role to admin user';
END
ELSE
BEGIN
    PRINT 'ℹ️  Admin role is already assigned';
END
PRINT '';

-- Show final status
PRINT '=============================================';
PRINT 'Final Status:';
PRINT '=============================================';
PRINT '';

PRINT 'Admin User Information:';
SELECT 
    UserId,
    Username,
    Email,
    IsEmailConfirmed,
    IsActive
FROM Users
WHERE UserId = @AdminUserId;
PRINT '';

PRINT 'Admin User Roles:';
SELECT 
    r.RoleName,
    r.Description
FROM UserRoles ur
INNER JOIN Roles r ON ur.RoleId = r.RoleId
WHERE ur.UserId = @AdminUserId;
PRINT '';

PRINT '=============================================';
PRINT '✅ Admin user fix completed!';
PRINT '=============================================';
PRINT '';
PRINT 'You can now log in with:';
PRINT '  Username: ' + (SELECT Username FROM Users WHERE UserId = @AdminUserId);
PRINT '  Email: ' + (SELECT Email FROM Users WHERE UserId = @AdminUserId);
PRINT '';
PRINT 'After login, you should be redirected to: /Admin/Dashboard';
PRINT '=============================================';

