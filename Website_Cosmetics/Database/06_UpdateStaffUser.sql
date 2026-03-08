-- =============================================
-- Update Staff User Script
-- =============================================
-- This script updates the manually created 'staff' user:
-- 1. Sets IsEmailConfirmed = 1
-- 2. Ensures Staff role is assigned
-- 3. Shows current user status
--
-- Run this script after manually creating the staff user

USE WebsiteCosmetic;
GO

SET QUOTED_IDENTIFIER ON;
GO

PRINT '=============================================';
PRINT 'Updating Staff User';
PRINT '=============================================';
PRINT '';

DECLARE @StaffUserId INT;
DECLARE @StaffUsername NVARCHAR(50) = 'staff';
DECLARE @StaffEmail NVARCHAR(100);
DECLARE @StaffRoleId INT;

-- Get Staff User information
SELECT @StaffUserId = UserId, @StaffEmail = Email
FROM Users
WHERE Username = @StaffUsername OR Email LIKE '%staff%';

IF @StaffUserId IS NULL
BEGIN
    PRINT '❌ Staff user not found. Please ensure the user exists.';
    PRINT '';
    PRINT 'Current users in database:';
    SELECT UserId, Username, Email, IsEmailConfirmed, IsActive
    FROM Users;
    PRINT '';
    PRINT '=============================================';
    RETURN;
END

DECLARE @StaffCurrentEmailConfirmed BIT;

-- Get additional user info
SELECT @StaffEmail = Email, @StaffCurrentEmailConfirmed = IsEmailConfirmed
FROM Users
WHERE UserId = @StaffUserId;

PRINT 'Found Staff User:';
PRINT '  UserId: ' + CAST(@StaffUserId AS NVARCHAR(10));
PRINT '  Username: ' + @StaffUsername;
PRINT '  Email: ' + ISNULL(@StaffEmail, 'N/A');
PRINT '  Current IsEmailConfirmed: ' + CAST(@StaffCurrentEmailConfirmed AS NVARCHAR(1));
PRINT '';

-- Get Staff Role ID
SELECT @StaffRoleId = RoleId
FROM Roles
WHERE RoleName = 'Staff';

IF @StaffRoleId IS NULL
BEGIN
    PRINT '❌ Role "Staff" not found. Please ensure the "Staff" role exists.';
    PRINT '   You might need to run 04_InsertAuthenticationData.sql first.';
    PRINT '';
    PRINT 'Current roles in database:';
    SELECT RoleId, RoleName, Description FROM Roles;
    PRINT '';
    PRINT '=============================================';
    RETURN;
END

PRINT 'Found Staff Role:';
PRINT '  RoleId: ' + CAST(@StaffRoleId AS NVARCHAR(10));
PRINT '  RoleName: Staff';
PRINT '';

-- 1. Update IsEmailConfirmed = 1
IF EXISTS (SELECT 1 FROM Users WHERE UserId = @StaffUserId AND IsEmailConfirmed = 0)
BEGIN
    UPDATE Users
    SET IsEmailConfirmed = 1, UpdatedAt = GETDATE()
    WHERE UserId = @StaffUserId;
    PRINT '✅ Updated IsEmailConfirmed to 1 for staff user.';
END
ELSE
BEGIN
    PRINT 'ℹ️  IsEmailConfirmed is already 1 for staff user.';
END

-- 2. Assign Staff role if not assigned
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @StaffUserId AND RoleId = @StaffRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, CreatedAt, UpdatedAt)
    VALUES (@StaffUserId, @StaffRoleId, GETDATE(), GETDATE());
    PRINT '✅ Assigned "Staff" role to staff user.';
END
ELSE
BEGIN
    PRINT 'ℹ️  Staff user already has the "Staff" role.';
END

PRINT '';

-- Show final status
PRINT '=============================================';
PRINT 'Final Staff User Status:';
PRINT '=============================================';

SELECT 
    u.UserId,
    u.Username,
    u.Email,
    u.FirstName,
    u.LastName,
    u.IsEmailConfirmed,
    u.IsActive,
    u.CreatedAt,
    u.UpdatedAt
FROM Users u
WHERE u.UserId = @StaffUserId;

PRINT '';
PRINT 'Staff User Roles:';

SELECT 
    r.RoleId,
    r.RoleName,
    r.Description
FROM UserRoles ur
INNER JOIN Roles r ON ur.RoleId = r.RoleId
WHERE ur.UserId = @StaffUserId;

PRINT '';
PRINT '=============================================';
PRINT '✅ Staff user update completed!';
PRINT '=============================================';
PRINT '';
PRINT 'The staff user has been updated:';
PRINT '  - IsEmailConfirmed: 1';
PRINT '  - Staff role: Assigned';
PRINT '';
PRINT 'The staff user can now log in and will be redirected to /Admin/StaffDashboard';
PRINT '=============================================';

