-- Inventory_StockLedgers: immutable movement audit trail (kartu stok), append-only (see docs/modules/inventory.md).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_StockLedgers')
BEGIN
    CREATE TABLE Inventory_StockLedgers
    (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_StockLedgers PRIMARY KEY,
        ItemId          BIGINT         NOT NULL,
        WarehouseId     BIGINT         NOT NULL,
        BatchId         BIGINT         NULL,
        MovementType    NVARCHAR(20)   NOT NULL,
        QuantityChange  DECIMAL(18,3)  NOT NULL,
        UnitCost        DECIMAL(18,4)  NOT NULL CONSTRAINT DF_Inventory_StockLedgers_UnitCost DEFAULT (0),
        -- Polymorphic pointer to the source transaction (Mutation/Opname/etc.) - resolved by string discriminator,
        -- not FK, since the source may live in another module (see docs/modules/inventory.md contract surface).
        ReferenceType   NVARCHAR(30)   NULL,
        ReferenceId     BIGINT         NULL,
        MovementDate    DATETIME2      NOT NULL,

        CreatedAt       DATETIME2      NOT NULL CONSTRAINT DF_Inventory_StockLedgers_CreatedAt DEFAULT (GETDATE()),
        CreatedBy       BIGINT         NULL,
        UpdatedAt       DATETIME2      NULL,
        UpdatedBy       BIGINT         NULL,
        IsDeleted       BIT            NOT NULL CONSTRAINT DF_Inventory_StockLedgers_IsDeleted DEFAULT (0),
        DeletedAt       DATETIME2      NULL,
        DeletedBy       BIGINT         NULL,

        CONSTRAINT CK_Inventory_StockLedgers_MovementType CHECK (MovementType IN
            ('Receipt', 'Mutation', 'ToVendor', 'FromVendor', 'Consumption', 'Adjustment', 'Reservation', 'Release', 'Shipped')),
        CONSTRAINT FK_Inventory_StockLedgers_Item FOREIGN KEY (ItemId) REFERENCES MasterData_Items (Id),
        CONSTRAINT FK_Inventory_StockLedgers_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Setting_Warehouses (Id),
        CONSTRAINT FK_Inventory_StockLedgers_Batch FOREIGN KEY (BatchId) REFERENCES Inventory_Batches (Id)
    );

    CREATE INDEX IX_Inventory_StockLedgers_Item_Warehouse ON Inventory_StockLedgers (ItemId, WarehouseId);
    CREATE INDEX IX_Inventory_StockLedgers_Reference ON Inventory_StockLedgers (ReferenceType, ReferenceId);
END
