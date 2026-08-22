-- Inventory_StockBalances: current on-hand snapshot per item/warehouse/batch, incl. weighted-average cost (see docs/modules/inventory.md).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_StockBalances')
BEGIN
    CREATE TABLE Inventory_StockBalances
    (
        Id                    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_StockBalances PRIMARY KEY,
        ItemId                BIGINT         NOT NULL,
        WarehouseId           BIGINT         NOT NULL,
        BatchId               BIGINT         NULL,
        UnitOfMeasurementId   BIGINT         NOT NULL,
        QuantityOnHand        DECIMAL(18,3)  NOT NULL CONSTRAINT DF_Inventory_StockBalances_QuantityOnHand DEFAULT (0),
        QuantityReserved      DECIMAL(18,3)  NOT NULL CONSTRAINT DF_Inventory_StockBalances_QuantityReserved DEFAULT (0),
        QuantityInQuarantine  DECIMAL(18,3)  NOT NULL CONSTRAINT DF_Inventory_StockBalances_QuantityInQuarantine DEFAULT (0),
        -- Weighted-average unit cost, recomputed on each Receipt: ((OldQty * OldAvgCost) + (ReceiptQty * ReceiptUnitCost)) / (OldQty + ReceiptQty).
        AverageCost           DECIMAL(18,4)  NOT NULL CONSTRAINT DF_Inventory_StockBalances_AverageCost DEFAULT (0),

        CreatedAt             DATETIME2      NOT NULL CONSTRAINT DF_Inventory_StockBalances_CreatedAt DEFAULT (GETDATE()),
        CreatedBy             BIGINT         NULL,
        UpdatedAt             DATETIME2      NULL,
        UpdatedBy             BIGINT         NULL,
        IsDeleted             BIT            NOT NULL CONSTRAINT DF_Inventory_StockBalances_IsDeleted DEFAULT (0),
        DeletedAt             DATETIME2      NULL,
        DeletedBy             BIGINT         NULL,

        CONSTRAINT FK_Inventory_StockBalances_Item FOREIGN KEY (ItemId) REFERENCES MasterData_Items (Id),
        CONSTRAINT FK_Inventory_StockBalances_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Setting_Warehouses (Id),
        CONSTRAINT FK_Inventory_StockBalances_Batch FOREIGN KEY (BatchId) REFERENCES Inventory_Batches (Id),
        CONSTRAINT FK_Inventory_StockBalances_UnitOfMeasurement FOREIGN KEY (UnitOfMeasurementId) REFERENCES Setting_UnitsOfMeasurement (Id)
    );

    -- Batch is nullable, so a plain UNIQUE constraint would only enforce uniqueness once per NULL (SQL Server treats
    -- each NULL as distinct); this filtered index instead enforces "one row per Item+Warehouse+Batch" including the
    -- batch-less case (NULL BatchId collapses to a single row per Item+Warehouse).
    CREATE UNIQUE INDEX UQ_Inventory_StockBalances_Item_Warehouse_Batch
        ON Inventory_StockBalances (ItemId, WarehouseId, BatchId)
        WHERE IsDeleted = 0;
END
