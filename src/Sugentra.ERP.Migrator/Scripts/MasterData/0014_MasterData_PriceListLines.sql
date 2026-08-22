-- MasterData_PriceListLines: item-level prices under a PriceListHeader (replaces the old flat MasterData_PriceLists).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MasterData_PriceListLines')
BEGIN
    CREATE TABLE MasterData_PriceListLines
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MasterData_PriceListLines PRIMARY KEY,
        PriceListHeaderId   BIGINT         NOT NULL,
        ItemId              BIGINT         NOT NULL,
        Price               DECIMAL(18,2)  NOT NULL,
        EffectiveDate       DATE           NOT NULL CONSTRAINT DF_MasterData_PriceListLines_EffectiveDate DEFAULT (CAST(GETDATE() AS DATE)),

        CreatedAt           DATETIME2      NOT NULL CONSTRAINT DF_MasterData_PriceListLines_CreatedAt DEFAULT (GETDATE()),
        CreatedBy           BIGINT         NULL,
        UpdatedAt           DATETIME2      NULL,
        UpdatedBy           BIGINT         NULL,
        IsDeleted           BIT            NOT NULL CONSTRAINT DF_MasterData_PriceListLines_IsDeleted DEFAULT (0),
        DeletedAt           DATETIME2      NULL,
        DeletedBy           BIGINT         NULL,

        CONSTRAINT UQ_MasterData_PriceListLines_Header_Item_Date UNIQUE (PriceListHeaderId, ItemId, EffectiveDate),
        CONSTRAINT FK_MasterData_PriceListLines_Header FOREIGN KEY (PriceListHeaderId) REFERENCES MasterData_PriceListHeaders (Id),
        CONSTRAINT FK_MasterData_PriceListLines_Item FOREIGN KEY (ItemId) REFERENCES MasterData_Items (Id)
    );
END
