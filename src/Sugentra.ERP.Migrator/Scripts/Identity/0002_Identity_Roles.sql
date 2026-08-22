-- Identity_Roles
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Identity_Roles')
BEGIN
    CREATE TABLE Identity_Roles
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Identity_Roles PRIMARY KEY,
        Name          NVARCHAR(100)  NOT NULL,
        Description   NVARCHAR(500)  NULL,

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Identity_Roles_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Identity_Roles_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT UQ_Identity_Roles_Name UNIQUE (Name)
    );
END
