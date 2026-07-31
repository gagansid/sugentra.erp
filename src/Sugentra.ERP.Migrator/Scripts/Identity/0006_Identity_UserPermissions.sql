-- Identity_UserPermissions: per-user allow/deny override on top of role-derived permissions. Deny always wins (see PermissionResolverService).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Identity_UserPermissions')
BEGIN
    CREATE TABLE Identity_UserPermissions
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Identity_UserPermissions PRIMARY KEY,
        UserId        BIGINT         NOT NULL,
        PermissionId  BIGINT         NOT NULL,
        IsAllowed     BIT            NOT NULL,

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Identity_UserPermissions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Identity_UserPermissions_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,
        RowVersion    ROWVERSION     NOT NULL,

        CONSTRAINT FK_Identity_UserPermissions_User FOREIGN KEY (UserId) REFERENCES Identity_Users (Id),
        CONSTRAINT FK_Identity_UserPermissions_Permission FOREIGN KEY (PermissionId) REFERENCES Identity_Permissions (Id),
        CONSTRAINT UQ_Identity_UserPermissions_User_Permission UNIQUE (UserId, PermissionId)
    );
END
