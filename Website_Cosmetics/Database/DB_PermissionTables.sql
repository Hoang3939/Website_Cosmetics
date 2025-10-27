-- Script tạo các bảng phân quyền chi tiết
-- Database: WebsiteCosmetic

-- Tạo bảng Permissions (Quyền hạn)
CREATE TABLE Permissions (
    UID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PermissionName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    Category NVARCHAR(50), -- Admin, Staff, User
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);

-- Tạo bảng RolePermissions (Quyền hạn của từng vai trò)
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

-- Tạo bảng UserPermissions (Quyền hạn cá nhân của user)
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

-- Tạo indexes
CREATE INDEX IX_Permissions_PermissionName ON Permissions(PermissionName);
CREATE INDEX IX_Permissions_Category ON Permissions(Category);
CREATE INDEX IX_RolePermissions_RoleUID ON RolePermissions(RoleUID);
CREATE INDEX IX_RolePermissions_PermissionUID ON RolePermissions(PermissionUID);
CREATE INDEX IX_UserPermissions_UserUID ON UserPermissions(UserUID);
CREATE INDEX IX_UserPermissions_PermissionUID ON UserPermissions(PermissionUID);
CREATE INDEX IX_UserPermissions_GrantedBy ON UserPermissions(GrantedBy);
