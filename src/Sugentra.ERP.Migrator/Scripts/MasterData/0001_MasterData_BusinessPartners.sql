-- MasterData_BusinessPartners: Customers and/or Suppliers (single table, PartnerType flag).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MasterData_BusinessPartners')
BEGIN
    CREATE TABLE MasterData_BusinessPartners
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MasterData_BusinessPartners PRIMARY KEY,
        Code          NVARCHAR(30)   NOT NULL,
        Name          NVARCHAR(200)  NOT NULL,
        PartnerType   NVARCHAR(20)   NOT NULL,
        Address       NVARCHAR(500)  NULL,
        City          NVARCHAR(100)  NULL,
        Country       NVARCHAR(100)  NULL,
        PhoneNumber   NVARCHAR(30)   NULL,
        Email         NVARCHAR(200)  NULL,
        TaxId         NVARCHAR(50)   NULL,
        ContactPerson NVARCHAR(200)  NULL,
        IsActive      BIT            NOT NULL CONSTRAINT DF_MasterData_BusinessPartners_IsActive DEFAULT (1),

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_MasterData_BusinessPartners_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_MasterData_BusinessPartners_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT UQ_MasterData_BusinessPartners_Code UNIQUE (Code),
        CONSTRAINT CK_MasterData_BusinessPartners_PartnerType CHECK (PartnerType IN ('Customer', 'Supplier', 'Both'))
    );
END
