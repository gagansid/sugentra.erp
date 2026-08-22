-- MasterData_PriceListHeaders: a named Sales or Purchase price list, optionally scoped to one business partner
-- (customer-specific / vendor-specific list); NULL BusinessPartnerId = general list for that Type + Currency.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MasterData_PriceListHeaders')
BEGIN
    CREATE TABLE MasterData_PriceListHeaders
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MasterData_PriceListHeaders PRIMARY KEY,
        Name                NVARCHAR(200)  NOT NULL,
        Type                NVARCHAR(20)   NOT NULL,
        CurrencyId          BIGINT         NOT NULL,
        BusinessPartnerId   BIGINT         NULL,
        IsActive            BIT            NOT NULL CONSTRAINT DF_MasterData_PriceListHeaders_IsActive DEFAULT (1),

        CreatedAt           DATETIME2      NOT NULL CONSTRAINT DF_MasterData_PriceListHeaders_CreatedAt DEFAULT (GETDATE()),
        CreatedBy           BIGINT         NULL,
        UpdatedAt           DATETIME2      NULL,
        UpdatedBy           BIGINT         NULL,
        IsDeleted           BIT            NOT NULL CONSTRAINT DF_MasterData_PriceListHeaders_IsDeleted DEFAULT (0),
        DeletedAt           DATETIME2      NULL,
        DeletedBy           BIGINT         NULL,

        CONSTRAINT FK_MasterData_PriceListHeaders_Currency FOREIGN KEY (CurrencyId) REFERENCES Setting_Currencies (Id),
        CONSTRAINT FK_MasterData_PriceListHeaders_BusinessPartner FOREIGN KEY (BusinessPartnerId) REFERENCES MasterData_BusinessPartners (Id)
    );

    ALTER TABLE MasterData_PriceListHeaders
        ADD CONSTRAINT CK_MasterData_PriceListHeaders_Type CHECK (Type IN ('Sales', 'Purchase'));
END
