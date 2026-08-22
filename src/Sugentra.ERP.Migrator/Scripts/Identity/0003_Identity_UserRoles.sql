-- Identity_UserRoles: junction table, User <-> Role (many-to-many)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Identity_UserRoles')
BEGIN
    CREATE TABLE Identity_UserRoles
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Identity_UserRoles PRIMARY KEY,
        UserId        BIGINT         NOT NULL,
        RoleId        BIGINT         NOT NULL,

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Identity_UserRoles_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Identity_UserRoles_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT FK_Identity_UserRoles_User FOREIGN KEY (UserId) REFERENCES Identity_Users (Id),
        CONSTRAINT FK_Identity_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES Identity_Roles (Id),
        CONSTRAINT UQ_Identity_UserRoles_User_Role UNIQUE (UserId, RoleId)
    );
END
