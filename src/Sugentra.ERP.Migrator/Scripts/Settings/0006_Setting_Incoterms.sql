-- Setting_Incoterms: e.g. FOB, CIF, EXW — used by future Sales/Export Order module.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_Incoterms')
BEGIN
    CREATE TABLE Setting_Incoterms
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_Incoterms PRIMARY KEY,
        Code          NVARCHAR(10)   NOT NULL,
        Name          NVARCHAR(200)  NOT NULL,
        IsActive      BIT            NOT NULL CONSTRAINT DF_Setting_Incoterms_IsActive DEFAULT (1),

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Setting_Incoterms_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Setting_Incoterms_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT UQ_Setting_Incoterms_Code UNIQUE (Code)
    );
END
