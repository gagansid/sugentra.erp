-- MasterData_PriceLists: one general price per Item + Currency (not customer-specific yet).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MasterData_PriceLists')
BEGIN
    CREATE TABLE MasterData_PriceLists
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MasterData_PriceLists PRIMARY KEY,
        ItemId          BIGINT         NOT NULL,
        CurrencyId      BIGINT         NOT NULL,
        Price           DECIMAL(18,2)  NOT NULL,
        EffectiveDate   DATE           NOT NULL CONSTRAINT DF_MasterData_PriceLists_EffectiveDate DEFAULT (CAST(GETDATE() AS DATE)),
        IsActive        BIT            NOT NULL CONSTRAINT DF_MasterData_PriceLists_IsActive DEFAULT (1),

        CreatedAt       DATETIME2      NOT NULL CONSTRAINT DF_MasterData_PriceLists_CreatedAt DEFAULT (GETDATE()),
        CreatedBy       BIGINT         NULL,
        UpdatedAt       DATETIME2      NULL,
        UpdatedBy       BIGINT         NULL,
        IsDeleted       BIT            NOT NULL CONSTRAINT DF_MasterData_PriceLists_IsDeleted DEFAULT (0),
        DeletedAt       DATETIME2      NULL,
        DeletedBy       BIGINT         NULL,

        CONSTRAINT UQ_MasterData_PriceLists_Item_Currency_Date UNIQUE (ItemId, CurrencyId, EffectiveDate),
        CONSTRAINT FK_MasterData_PriceLists_Item FOREIGN KEY (ItemId) REFERENCES MasterData_Items (Id),
        CONSTRAINT FK_MasterData_PriceLists_Currency FOREIGN KEY (CurrencyId) REFERENCES Setting_Currencies (Id)
    );
END
