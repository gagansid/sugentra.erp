-- MasterData_BusinessPartnerAddresses: one-to-many addresses per partner (Office/Billing/Shipping/Other).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MasterData_BusinessPartnerAddresses')
BEGIN
    CREATE TABLE MasterData_BusinessPartnerAddresses
    (
        Id                BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MasterData_BusinessPartnerAddresses PRIMARY KEY,
        BusinessPartnerId BIGINT         NOT NULL,
        AddressType       NVARCHAR(20)   NOT NULL,
        Address           NVARCHAR(500)  NOT NULL,
        City              NVARCHAR(100)  NULL,
        Province          NVARCHAR(100)  NULL,
        PostalCode        NVARCHAR(20)   NULL,
        Country           NVARCHAR(100)  NULL,
        IsPrimary         BIT            NOT NULL CONSTRAINT DF_MasterData_BusinessPartnerAddresses_IsPrimary DEFAULT (0),

        CreatedAt         DATETIME2      NOT NULL CONSTRAINT DF_MasterData_BusinessPartnerAddresses_CreatedAt DEFAULT (GETDATE()),
        CreatedBy         BIGINT         NULL,
        UpdatedAt         DATETIME2      NULL,
        UpdatedBy         BIGINT         NULL,
        IsDeleted         BIT            NOT NULL CONSTRAINT DF_MasterData_BusinessPartnerAddresses_IsDeleted DEFAULT (0),
        DeletedAt         DATETIME2      NULL,
        DeletedBy         BIGINT         NULL,

        CONSTRAINT FK_MasterData_BusinessPartnerAddresses_Partner FOREIGN KEY (BusinessPartnerId) REFERENCES MasterData_BusinessPartners (Id),
        CONSTRAINT CK_MasterData_BusinessPartnerAddresses_AddressType CHECK (AddressType IN ('Office', 'Billing', 'Shipping', 'Other'))
    );
END
