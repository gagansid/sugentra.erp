-- Identity_Permissions: catalog of permission codes (e.g. "Create_Invoice") used as JWT claims/policy names.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Identity_Permissions')
BEGIN
    CREATE TABLE Identity_Permissions
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Identity_Permissions PRIMARY KEY,
        Code          NVARCHAR(150)  NOT NULL,
        Module        NVARCHAR(100)  NULL,
        Description   NVARCHAR(500)  NULL,

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Identity_Permissions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Identity_Permissions_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,
        RowVersion    ROWVERSION     NOT NULL,

        CONSTRAINT UQ_Identity_Permissions_Code UNIQUE (Code)
    );
END
