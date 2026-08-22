-- Identity_RolePermissions: junction table, base permission grants per Role
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Identity_RolePermissions')
BEGIN
    CREATE TABLE Identity_RolePermissions
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Identity_RolePermissions PRIMARY KEY,
        RoleId        BIGINT         NOT NULL,
        PermissionId  BIGINT         NOT NULL,

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Identity_RolePermissions_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Identity_RolePermissions_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT FK_Identity_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES Identity_Roles (Id),
        CONSTRAINT FK_Identity_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES Identity_Permissions (Id),
        CONSTRAINT UQ_Identity_RolePermissions_Role_Permission UNIQUE (RoleId, PermissionId)
    );
END
