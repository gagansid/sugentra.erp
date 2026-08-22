-- Setting_CompanyProfile: singleton (single row, Id=1), the exporter's own company info used on export documents.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_CompanyProfile')
BEGIN
    CREATE TABLE Setting_CompanyProfile
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_CompanyProfile PRIMARY KEY,
        CompanyName   NVARCHAR(200)  NOT NULL,
        LegalName     NVARCHAR(200)  NULL,
        TaxId         NVARCHAR(50)   NULL,
        Address       NVARCHAR(500)  NULL,
        City          NVARCHAR(100)  NULL,
        Country       NVARCHAR(100)  NULL,
        PhoneNumber   NVARCHAR(50)   NULL,
        Email         NVARCHAR(256)  NULL,
        Website       NVARCHAR(200)  NULL,
        LogoUrl       NVARCHAR(500)  NULL,

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Setting_CompanyProfile_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Setting_CompanyProfile_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL
    );
END
