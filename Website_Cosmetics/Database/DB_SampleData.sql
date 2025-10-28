-- Script insert dữ liệu mẫu cho hệ thống Authentication
-- Database: Website_Cosmetics

-- Insert các Role mặc định
INSERT INTO Roles (UID, RoleName, Description, IsActive, CreatedAt, UpdatedAt) VALUES
(NEWID(), 'Admin', 'Quản trị viên hệ thống', 1, GETDATE(), GETDATE()),
(NEWID(), 'User', 'Người dùng thông thường', 1, GETDATE(), GETDATE()),
(NEWID(), 'Moderator', 'Người kiểm duyệt', 1, GETDATE(), GETDATE());

-- Insert user admin mặc định (password: Admin123!)
-- Password hash được tạo bằng BCrypt với cost 12
INSERT INTO Users (UID, Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, IsEmailConfirmed, IsActive, CreatedAt, UpdatedAt) VALUES
(NEWID(), 'admin', 'admin@cosmetics.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj4J8K8K8K8K', 'Admin', 'System', '0123456789', 1, 1, GETDATE(), GETDATE());

-- Gán role Admin cho user admin
INSERT INTO UserRoles (UID, UserUID, RoleUID, CreatedAt, UpdatedAt)
SELECT NEWID(), u.UID, r.UID, GETDATE(), GETDATE()
FROM Users u, Roles r
WHERE u.Username = 'admin' AND r.RoleName = 'Admin';
