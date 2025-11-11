-- =============================================
-- Delete Staff User Script
-- =============================================
-- This script deletes the 'staff' user and all associated data:
-- - UserRoles (role assignments)
-- - UserPermissions (direct permission assignments)
-- - EmailConfirmationTokens
-- - PasswordResetTokens
-- - Users
--
-- Run this script if you want to manually create the staff user later

USE WebsiteCosmetic;
GO

SET QUOTED_IDENTIFIER ON;
GO

PRINT '=============================================';
PRINT 'Deleting Staff User';
PRINT '=============================================';
PRINT '';

DECLARE @StaffUserId INT;
DECLARE @StaffUsername NVARCHAR(50) = 'staff';
DECLARE @StaffEmail NVARCHAR(100);

-- Get Staff User ID and Email
SELECT @StaffUserId = UserId, @StaffEmail = Email
FROM Users
WHERE Username = @StaffUsername;

IF @StaffUserId IS NULL
BEGIN
    PRINT 'ℹ️  Staff user (' + @StaffUsername + ') not found. Nothing to delete.';
    PRINT '';
    PRINT 'Current users in database:';
    SELECT UserId, Username, Email, IsActive 
    FROM Users;
    PRINT '';
    PRINT '=============================================';
    RETURN;
END

PRINT 'Found Staff User:';
PRINT '  UserId: ' + CAST(@StaffUserId AS NVARCHAR(10));
PRINT '  Username: ' + @StaffUsername;
PRINT '  Email: ' + ISNULL(@StaffEmail, 'N/A');
PRINT '';

-- Show what will be deleted
PRINT 'Checking related data...';

DECLARE @UserRolesCount INT = (SELECT COUNT(*) FROM UserRoles WHERE UserId = @StaffUserId);
DECLARE @UserPermissionsCount INT = (SELECT COUNT(*) FROM UserPermissions WHERE UserId = @StaffUserId);
DECLARE @EmailTokensCount INT = (SELECT COUNT(*) FROM EmailConfirmationTokens WHERE UserId = @StaffUserId);
DECLARE @PasswordTokensCount INT = (SELECT COUNT(*) FROM PasswordResetTokens WHERE UserId = @StaffUserId);

PRINT '  - UserRoles records: ' + CAST(@UserRolesCount AS NVARCHAR(10));
PRINT '  - UserPermissions records: ' + CAST(@UserPermissionsCount AS NVARCHAR(10));
PRINT '  - EmailConfirmationTokens records: ' + CAST(@EmailTokensCount AS NVARCHAR(10));
PRINT '  - PasswordResetTokens records: ' + CAST(@PasswordTokensCount AS NVARCHAR(10));
PRINT '';

-- Delete in order (respecting foreign key constraints)

-- 1. Delete UserPermissions (if any)
IF @UserPermissionsCount > 0
BEGIN
    DELETE FROM UserPermissions
    WHERE UserId = @StaffUserId;
    PRINT '✓ Deleted ' + CAST(@UserPermissionsCount AS NVARCHAR(10)) + ' UserPermissions record(s).';
END

-- 2. Delete UserRoles
IF @UserRolesCount > 0
BEGIN
    DELETE FROM UserRoles
    WHERE UserId = @StaffUserId;
    PRINT '✓ Deleted ' + CAST(@UserRolesCount AS NVARCHAR(10)) + ' UserRoles record(s).';
END

-- 3. Delete EmailConfirmationTokens
IF @EmailTokensCount > 0
BEGIN
    DELETE FROM EmailConfirmationTokens
    WHERE UserId = @StaffUserId;
    PRINT '✓ Deleted ' + CAST(@EmailTokensCount AS NVARCHAR(10)) + ' EmailConfirmationTokens record(s).';
END

-- 4. Delete PasswordResetTokens
IF @PasswordTokensCount > 0
BEGIN
    DELETE FROM PasswordResetTokens
    WHERE UserId = @StaffUserId;
    PRINT '✓ Deleted ' + CAST(@PasswordTokensCount AS NVARCHAR(10)) + ' PasswordResetTokens record(s).';
END

-- 5. Delete User (CASCADE will handle related tables automatically)
DELETE FROM Users
WHERE UserId = @StaffUserId;

IF @@ROWCOUNT > 0
BEGIN
    PRINT '✓ Deleted Staff User from Users table.';
END

PRINT '';
PRINT '=============================================';
PRINT 'Staff User Deletion Complete!';
PRINT '=============================================';
PRINT '';
PRINT 'The staff user and all associated data have been deleted.';
PRINT 'You can now create the staff user manually.';
PRINT '';
PRINT 'Remaining users:';
SELECT UserId, Username, Email, IsEmailConfirmed, IsActive 
FROM Users
ORDER BY UserId;
PRINT '=============================================';

