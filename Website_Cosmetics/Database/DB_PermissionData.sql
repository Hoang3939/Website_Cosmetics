-- Script insert dữ liệu phân quyền mẫu
-- Database: WebsiteCosmetic

-- Insert các Permissions
INSERT INTO Permissions (UID, PermissionName, Description, Category, IsActive, CreatedAt, UpdatedAt) VALUES
-- Admin Permissions
(NEWID(), 'Admin.Dashboard.View', 'Xem trang tổng quan admin', 'Admin', 1, GETDATE(), GETDATE()),
(NEWID(), 'Admin.Users.Manage', 'Quản lý người dùng', 'Admin', 1, GETDATE(), GETDATE()),
(NEWID(), 'Admin.Staff.Manage', 'Quản lý nhân viên', 'Admin', 1, GETDATE(), GETDATE()),
(NEWID(), 'Admin.Permissions.Manage', 'Quản lý quyền hạn', 'Admin', 1, GETDATE(), GETDATE()),
(NEWID(), 'Admin.Settings.Manage', 'Quản lý cài đặt hệ thống', 'Admin', 1, GETDATE(), GETDATE()),

-- Staff Permissions
(NEWID(), 'Staff.Dashboard.View', 'Xem trang tổng quan nhân viên', 'Staff', 1, GETDATE(), GETDATE()),
(NEWID(), 'Staff.Products.Manage', 'Quản lý sản phẩm', 'Staff', 1, GETDATE(), GETDATE()),
(NEWID(), 'Staff.Orders.Manage', 'Quản lý đơn hàng', 'Staff', 1, GETDATE(), GETDATE()),
(NEWID(), 'Staff.Customers.View', 'Xem thông tin khách hàng', 'Staff', 1, GETDATE(), GETDATE()),
(NEWID(), 'Staff.Reports.View', 'Xem báo cáo', 'Staff', 1, GETDATE(), GETDATE()),

-- User Permissions
(NEWID(), 'User.Profile.Manage', 'Quản lý thông tin cá nhân', 'User', 1, GETDATE(), GETDATE()),
(NEWID(), 'User.Orders.View', 'Xem đơn hàng của mình', 'User', 1, GETDATE(), GETDATE()),
(NEWID(), 'User.Products.View', 'Xem sản phẩm', 'User', 1, GETDATE(), GETDATE());

-- Gán quyền cho Role Admin
INSERT INTO RolePermissions (UID, RoleUID, PermissionUID, CreatedAt, UpdatedAt)
SELECT NEWID(), r.UID, p.UID, GETDATE(), GETDATE()
FROM Roles r, Permissions p
WHERE r.RoleName = 'Admin' AND p.Category = 'Admin';

-- Gán quyền cho Role Staff
INSERT INTO RolePermissions (UID, RoleUID, PermissionUID, CreatedAt, UpdatedAt)
SELECT NEWID(), r.UID, p.UID, GETDATE(), GETDATE()
FROM Roles r, Permissions p
WHERE r.RoleName = 'Staff' AND p.Category = 'Staff';

-- Gán quyền cho Role User
INSERT INTO RolePermissions (UID, RoleUID, PermissionUID, CreatedAt, UpdatedAt)
SELECT NEWID(), r.UID, p.UID, GETDATE(), GETDATE()
FROM Roles r, Permissions p
WHERE r.RoleName = 'User' AND p.Category = 'User';
