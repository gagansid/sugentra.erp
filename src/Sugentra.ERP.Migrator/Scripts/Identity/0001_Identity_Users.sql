-- Identity_Users: foundational table, every module's CreatedBy/UpdatedBy/DeletedBy points here.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Identity_Users')
BEGIN
    CREATE TABLE Identity_Users
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Identity_Users PRIMARY KEY,
        Username      NVARCHAR(100)  NOT NULL,
        Email         NVARCHAR(256)  NOT NULL,
        PasswordHash  NVARCHAR(256)  NOT NULL,
        FullName      NVARCHAR(200)  NOT NULL,
        IsActive      BIT            NOT NULL CONSTRAINT DF_Identity_Users_IsActive DEFAULT (1),

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Identity_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Identity_Users_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,
        RowVersion    ROWVERSION     NOT NULL,

        CONSTRAINT UQ_Identity_Users_Username UNIQUE (Username),
        CONSTRAINT UQ_Identity_Users_Email UNIQUE (Email)
    );
END
