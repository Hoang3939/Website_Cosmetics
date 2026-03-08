IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Roles] (
    [UID] uniqueidentifier NOT NULL,
    [RoleName] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_Roles] PRIMARY KEY ([UID])
);
GO

CREATE TABLE [Users] (
    [UID] uniqueidentifier NOT NULL,
    [Username] nvarchar(50) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [FirstName] nvarchar(50) NULL,
    [LastName] nvarchar(50) NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [IsEmailConfirmed] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_Users] PRIMARY KEY ([UID])
);
GO

CREATE TABLE [EmailConfirmationTokens] (
    [UID] uniqueidentifier NOT NULL,
    [UserUID] uniqueidentifier NOT NULL,
    [Token] nvarchar(255) NOT NULL,
    [ExpiresAt] datetime2(7) NOT NULL,
    [IsUsed] bit NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_EmailConfirmationTokens] PRIMARY KEY ([UID]),
    CONSTRAINT [FK_EmailConfirmationTokens_Users_UserUID] FOREIGN KEY ([UserUID]) REFERENCES [Users] ([UID]) ON DELETE CASCADE
);
GO

CREATE TABLE [PasswordResetTokens] (
    [UID] uniqueidentifier NOT NULL,
    [UserUID] uniqueidentifier NOT NULL,
    [Token] nvarchar(255) NOT NULL,
    [ExpiresAt] datetime2(7) NOT NULL,
    [IsUsed] bit NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([UID]),
    CONSTRAINT [FK_PasswordResetTokens_Users_UserUID] FOREIGN KEY ([UserUID]) REFERENCES [Users] ([UID]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserRoles] (
    [UID] uniqueidentifier NOT NULL,
    [UserUID] uniqueidentifier NOT NULL,
    [RoleUID] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UID]),
    CONSTRAINT [FK_UserRoles_Roles_RoleUID] FOREIGN KEY ([RoleUID]) REFERENCES [Roles] ([UID]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_UserUID] FOREIGN KEY ([UserUID]) REFERENCES [Users] ([UID]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_EmailConfirmationTokens_Token] ON [EmailConfirmationTokens] ([Token]);
GO

CREATE INDEX [IX_EmailConfirmationTokens_UserUID] ON [EmailConfirmationTokens] ([UserUID]);
GO

CREATE UNIQUE INDEX [IX_PasswordResetTokens_Token] ON [PasswordResetTokens] ([Token]);
GO

CREATE INDEX [IX_PasswordResetTokens_UserUID] ON [PasswordResetTokens] ([UserUID]);
GO

CREATE UNIQUE INDEX [IX_Roles_RoleName] ON [Roles] ([RoleName]);
GO

CREATE INDEX [IX_UserRoles_RoleUID] ON [UserRoles] ([RoleUID]);
GO

CREATE UNIQUE INDEX [IX_UserRoles_UserUID_RoleUID] ON [UserRoles] ([UserUID], [RoleUID]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251027190805_DatabaseExists', N'8.0.0');
GO

COMMIT;
GO

