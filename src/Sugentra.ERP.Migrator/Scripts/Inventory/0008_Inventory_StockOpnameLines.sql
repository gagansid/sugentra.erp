-- Inventory_StockOpnameLines: system vs. counted qty per item/batch for a Stock Opname header (see docs/modules/inventory.md).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_StockOpnameLines')
BEGIN
    CREATE TABLE Inventory_StockOpnameLines
    (
        Id                BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_StockOpnameLines PRIMARY KEY,
        OpnameId          BIGINT         NOT NULL,
        ItemId            BIGINT         NOT NULL,
        BatchId           BIGINT         NULL,
        -- Snapshot of Inventory_StockBalances.QuantityOnHand at count time.
        SystemQuantity    DECIMAL(18,3)  NOT NULL,
        CountedQuantity   DECIMAL(18,3)  NOT NULL,
        -- Computed as CountedQuantity - SystemQuantity, stored (not PERSISTED) so app code owns the calculation.
        VarianceQuantity  DECIMAL(18,3)  NOT NULL,
        Notes             NVARCHAR(500)  NULL,

        CreatedAt         DATETIME2      NOT NULL CONSTRAINT DF_Inventory_StockOpnameLines_CreatedAt DEFAULT (GETDATE()),
        CreatedBy         BIGINT         NULL,
        UpdatedAt         DATETIME2      NULL,
        UpdatedBy         BIGINT         NULL,
        IsDeleted         BIT            NOT NULL CONSTRAINT DF_Inventory_StockOpnameLines_IsDeleted DEFAULT (0),
        DeletedAt         DATETIME2      NULL,
        DeletedBy         BIGINT         NULL,

        CONSTRAINT FK_Inventory_StockOpnameLines_Opname FOREIGN KEY (OpnameId) REFERENCES Inventory_StockOpnames (Id),
        CONSTRAINT FK_Inventory_StockOpnameLines_Item FOREIGN KEY (ItemId) REFERENCES MasterData_Items (Id),
        CONSTRAINT FK_Inventory_StockOpnameLines_Batch FOREIGN KEY (BatchId) REFERENCES Inventory_Batches (Id)
    );

    CREATE INDEX IX_Inventory_StockOpnameLines_Opname ON Inventory_StockOpnameLines (OpnameId);
END
