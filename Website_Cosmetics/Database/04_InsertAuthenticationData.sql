-- =============================================
-- Insert Authentication Data Only
-- =============================================
-- This script inserts data ONLY for authentication-related tables:
-- - Roles
-- - Permissions
-- - Users (Admin, Staff, User)
-- - UserRoles
-- - RolePermissions
--
-- Run this script AFTER running 01_CreateTables.sql
-- This script is idempotent (can be run multiple times safely)

USE WebsiteCosmetic;
GO

SET QUOTED_IDENTIFIER ON;
GO

PRINT '=============================================';
PRINT 'Inserting Authentication Data';
PRINT '=============================================';
PRINT '';

-- =============================================
-- Insert Roles Data
-- =============================================
PRINT '1. Inserting Roles...';

DECLARE @AdminRoleId INT;
DECLARE @StaffRoleId INT;
DECLARE @UserRoleId INT;

-- Insert Admin role
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO Roles (RoleName, Description, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Admin', 'Administrator with full system access', 1, GETDATE(), GETDATE());
    SET @AdminRoleId = SCOPE_IDENTITY();
    PRINT '   ✓ Admin role inserted.';
END
ELSE
BEGIN
    SELECT @AdminRoleId = RoleId FROM Roles WHERE RoleName = 'Admin';
    PRINT '   - Admin role already exists.';
END;

-- Insert Staff role
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Staff')
BEGIN
    INSERT INTO Roles (RoleName, Description, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Staff', 'Staff member with limited admin access', 1, GETDATE(), GETDATE());
    SET @StaffRoleId = SCOPE_IDENTITY();
    PRINT '   ✓ Staff role inserted.';
END
ELSE
BEGIN
    SELECT @StaffRoleId = RoleId FROM Roles WHERE RoleName = 'Staff';
    PRINT '   - Staff role already exists.';
END;

-- Insert User role
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'User')
BEGIN
    INSERT INTO Roles (RoleName, Description, IsActive, CreatedAt, UpdatedAt)
    VALUES ('User', 'Regular user with basic access', 1, GETDATE(), GETDATE());
    SET @UserRoleId = SCOPE_IDENTITY();
    PRINT '   ✓ User role inserted.';
END
ELSE
BEGIN
    SELECT @UserRoleId = RoleId FROM Roles WHERE RoleName = 'User';
    PRINT '   - User role already exists.';
END;

PRINT '';

-- =============================================
-- Insert Permissions Data
-- =============================================
PRINT '2. Inserting Permissions...';

DECLARE @AdminDashboardPermId INT;
DECLARE @StaffDashboardPermId INT;
DECLARE @UserProfilePermId INT;
DECLARE @ProductManagePermId INT;
DECLARE @CategoryManagePermId INT;
DECLARE @UserManagePermId INT;

-- Admin.Dashboard.View
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionName = 'Admin.Dashboard.View')
BEGIN
    INSERT INTO Permissions (PermissionName, Description, Category, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Admin.Dashboard.View', 'View admin dashboard', 'Admin', 1, GETDATE(), GETDATE());
    SET @AdminDashboardPermId = SCOPE_IDENTITY();
    PRINT '   ✓ Admin.Dashboard.View permission inserted.';
END
ELSE
BEGIN
    SELECT @AdminDashboardPermId = PermissionId FROM Permissions WHERE PermissionName = 'Admin.Dashboard.View';
    PRINT '   - Admin.Dashboard.View permission already exists.';
END;

-- Staff.Dashboard.View
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionName = 'Staff.Dashboard.View')
BEGIN
    INSERT INTO Permissions (PermissionName, Description, Category, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Staff.Dashboard.View', 'View staff dashboard', 'Staff', 1, GETDATE(), GETDATE());
    SET @StaffDashboardPermId = SCOPE_IDENTITY();
    PRINT '   ✓ Staff.Dashboard.View permission inserted.';
END
ELSE
BEGIN
    SELECT @StaffDashboardPermId = PermissionId FROM Permissions WHERE PermissionName = 'Staff.Dashboard.View';
    PRINT '   - Staff.Dashboard.View permission already exists.';
END;

-- User.Profile.View
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionName = 'User.Profile.View')
BEGIN
    INSERT INTO Permissions (PermissionName, Description, Category, IsActive, CreatedAt, UpdatedAt)
    VALUES ('User.Profile.View', 'View user profile', 'User', 1, GETDATE(), GETDATE());
    SET @UserProfilePermId = SCOPE_IDENTITY();
    PRINT '   ✓ User.Profile.View permission inserted.';
END
ELSE
BEGIN
    SELECT @UserProfilePermId = PermissionId FROM Permissions WHERE PermissionName = 'User.Profile.View';
    PRINT '   - User.Profile.View permission already exists.';
END;

-- Product.Manage
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionName = 'Product.Manage')
BEGIN
    INSERT INTO Permissions (PermissionName, Description, Category, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Product.Manage', 'Manage products', 'Admin', 1, GETDATE(), GETDATE());
    SET @ProductManagePermId = SCOPE_IDENTITY();
    PRINT '   ✓ Product.Manage permission inserted.';
END
ELSE
BEGIN
    SELECT @ProductManagePermId = PermissionId FROM Permissions WHERE PermissionName = 'Product.Manage';
    PRINT '   - Product.Manage permission already exists.';
END;

-- Category.Manage
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionName = 'Category.Manage')
BEGIN
    INSERT INTO Permissions (PermissionName, Description, Category, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Category.Manage', 'Manage categories', 'Admin', 1, GETDATE(), GETDATE());
    SET @CategoryManagePermId = SCOPE_IDENTITY();
    PRINT '   ✓ Category.Manage permission inserted.';
END
ELSE
BEGIN
    SELECT @CategoryManagePermId = PermissionId FROM Permissions WHERE PermissionName = 'Category.Manage';
    PRINT '   - Category.Manage permission already exists.';
END;

-- User.Manage
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionName = 'User.Manage')
BEGIN
    INSERT INTO Permissions (PermissionName, Description, Category, IsActive, CreatedAt, UpdatedAt)
    VALUES ('User.Manage', 'Manage users', 'Admin', 1, GETDATE(), GETDATE());
    SET @UserManagePermId = SCOPE_IDENTITY();
    PRINT '   ✓ User.Manage permission inserted.';
END
ELSE
BEGIN
    SELECT @UserManagePermId = PermissionId FROM Permissions WHERE PermissionName = 'User.Manage';
    PRINT '   - User.Manage permission already exists.';
END;

PRINT '';

-- =============================================
-- Insert Sample Users
-- =============================================
PRINT '3. Inserting Sample Users...';

DECLARE @AdminUserId INT;
DECLARE @StaffUserId INT;
DECLARE @RegularUserId INT;

-- Password hash for "Admin123!" (BCrypt)
DECLARE @PasswordHash NVARCHAR(255) = '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi';

-- Admin User
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, IsEmailConfirmed, IsActive, CreatedAt, UpdatedAt)
    VALUES ('admin', 'admin@cosmetics.com', @PasswordHash, 'Admin', 'User', 1, 1, GETDATE(), GETDATE());
    SET @AdminUserId = SCOPE_IDENTITY();
    PRINT '   ✓ Admin user inserted.';
END
ELSE
BEGIN
    SELECT @AdminUserId = UserId FROM Users WHERE Username = 'admin';
    -- Ensure IsEmailConfirmed = 1
    UPDATE Users SET IsEmailConfirmed = 1, UpdatedAt = GETDATE() WHERE UserId = @AdminUserId AND IsEmailConfirmed = 0;
    PRINT '   - Admin user already exists.';
END;

-- Staff User
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'staff')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, IsEmailConfirmed, IsActive, CreatedAt, UpdatedAt)
    VALUES ('staff', 'staff@cosmetics.com', @PasswordHash, 'Staff', 'User', 1, 1, GETDATE(), GETDATE());
    SET @StaffUserId = SCOPE_IDENTITY();
    PRINT '   ✓ Staff user inserted.';
END
ELSE
BEGIN
    SELECT @StaffUserId = UserId FROM Users WHERE Username = 'staff';
    -- Ensure IsEmailConfirmed = 1
    UPDATE Users SET IsEmailConfirmed = 1, UpdatedAt = GETDATE() WHERE UserId = @StaffUserId AND IsEmailConfirmed = 0;
    PRINT '   - Staff user already exists.';
END;

-- Regular User
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'user')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, IsEmailConfirmed, IsActive, CreatedAt, UpdatedAt)
    VALUES ('user', 'user@cosmetics.com', @PasswordHash, 'Regular', 'User', 1, 1, GETDATE(), GETDATE());
    SET @RegularUserId = SCOPE_IDENTITY();
    PRINT '   ✓ Regular user inserted.';
END
ELSE
BEGIN
    SELECT @RegularUserId = UserId FROM Users WHERE Username = 'user';
    -- Ensure IsEmailConfirmed = 1
    UPDATE Users SET IsEmailConfirmed = 1, UpdatedAt = GETDATE() WHERE UserId = @RegularUserId AND IsEmailConfirmed = 0;
    PRINT '   - Regular user already exists.';
END;

PRINT '';

-- =============================================
-- Assign User Roles
-- =============================================
PRINT '4. Assigning User Roles...';

-- Admin User -> Admin Role
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, CreatedAt, UpdatedAt)
    VALUES (@AdminUserId, @AdminRoleId, GETDATE(), GETDATE());
    PRINT '   ✓ Admin user assigned Admin role.';
END
ELSE
BEGIN
    PRINT '   - Admin user already has Admin role.';
END;

-- Staff User -> Staff Role
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @StaffUserId AND RoleId = @StaffRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, CreatedAt, UpdatedAt)
    VALUES (@StaffUserId, @StaffRoleId, GETDATE(), GETDATE());
    PRINT '   ✓ Staff user assigned Staff role.';
END
ELSE
BEGIN
    PRINT '   - Staff user already has Staff role.';
END;

-- Regular User -> User Role
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @RegularUserId AND RoleId = @UserRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, CreatedAt, UpdatedAt)
    VALUES (@RegularUserId, @UserRoleId, GETDATE(), GETDATE());
    PRINT '   ✓ Regular user assigned User role.';
END
ELSE
BEGIN
    PRINT '   - Regular user already has User role.';
END;

PRINT '';

-- =============================================
-- Assign Role Permissions
-- =============================================
PRINT '5. Assigning Role Permissions...';

-- Admin Role Permissions
IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @AdminDashboardPermId)
    INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt, UpdatedAt) VALUES (@AdminRoleId, @AdminDashboardPermId, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @ProductManagePermId)
    INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt, UpdatedAt) VALUES (@AdminRoleId, @ProductManagePermId, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @CategoryManagePermId)
    INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt, UpdatedAt) VALUES (@AdminRoleId, @CategoryManagePermId, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @UserManagePermId)
    INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt, UpdatedAt) VALUES (@AdminRoleId, @UserManagePermId, GETDATE(), GETDATE());

PRINT '   ✓ Admin role permissions assigned (4 permissions).';

-- Staff Role Permissions
IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = @StaffRoleId AND PermissionId = @StaffDashboardPermId)
    INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt, UpdatedAt) VALUES (@StaffRoleId, @StaffDashboardPermId, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = @StaffRoleId AND PermissionId = @AdminDashboardPermId)
    INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt, UpdatedAt) VALUES (@StaffRoleId, @AdminDashboardPermId, GETDATE(), GETDATE());

PRINT '   ✓ Staff role permissions assigned (2 permissions).';

-- User Role Permissions
IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = @UserRoleId AND PermissionId = @UserProfilePermId)
    INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt, UpdatedAt) VALUES (@UserRoleId, @UserProfilePermId, GETDATE(), GETDATE());

PRINT '   ✓ User role permissions assigned (1 permission).';

PRINT '';

-- =============================================
-- Summary
-- =============================================
PRINT '=============================================';
PRINT 'Authentication Data Insertion Complete!';
PRINT '=============================================';
PRINT '';
PRINT 'Summary:';
PRINT '  - Roles: 3 (Admin, Staff, User)';
PRINT '  - Permissions: 6';
PRINT '  - Users: 3 (admin, staff, user)';
PRINT '  - User Roles: 3 assignments';
PRINT '  - Role Permissions: 7 assignments';
PRINT '';
PRINT 'Sample Accounts:';
PRINT '  Admin  - Username: admin,  Password: Admin123!';
PRINT '  Staff  - Username: staff,  Password: Admin123!';
PRINT '  User   - Username: user,   Password: Admin123!';
PRINT '';
PRINT 'Note: All users have IsEmailConfirmed = 1';
PRINT '=============================================';


