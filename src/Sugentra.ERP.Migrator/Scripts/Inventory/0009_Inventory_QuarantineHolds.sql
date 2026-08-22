-- Inventory_QuarantineHolds: generic hold state for QC-failed batches, customer returns, and fumigation-pending finished goods (see docs/modules/inventory.md).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_QuarantineHolds')
BEGIN
    CREATE TABLE Inventory_QuarantineHolds
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_QuarantineHolds PRIMARY KEY,
        BatchId     BIGINT         NULL,
        ItemId      BIGINT         NOT NULL,
        WarehouseId BIGINT         NOT NULL,
        HoldReason  NVARCHAR(30)   NOT NULL,
        Status      NVARCHAR(20)   NOT NULL CONSTRAINT DF_Inventory_QuarantineHolds_Status DEFAULT ('OnHold'),
        PlacedAt    DATETIME2      NOT NULL CONSTRAINT DF_Inventory_QuarantineHolds_PlacedAt DEFAULT (GETDATE()),
        ReleasedAt  DATETIME2      NULL,
        ReleasedBy  BIGINT         NULL,
        Notes       NVARCHAR(500)  NULL,

        CreatedAt   DATETIME2      NOT NULL CONSTRAINT DF_Inventory_QuarantineHolds_CreatedAt DEFAULT (GETDATE()),
        CreatedBy   BIGINT         NULL,
        UpdatedAt   DATETIME2      NULL,
        UpdatedBy   BIGINT         NULL,
        IsDeleted   BIT            NOT NULL CONSTRAINT DF_Inventory_QuarantineHolds_IsDeleted DEFAULT (0),
        DeletedAt   DATETIME2      NULL,
        DeletedBy   BIGINT         NULL,

        CONSTRAINT CK_Inventory_QuarantineHolds_HoldReason CHECK (HoldReason IN ('QcFailed', 'CustomerReturn', 'FumigationPending')),
        CONSTRAINT CK_Inventory_QuarantineHolds_Status CHECK (Status IN ('OnHold', 'Released', 'Rejected')),
        CONSTRAINT FK_Inventory_QuarantineHolds_Batch FOREIGN KEY (BatchId) REFERENCES Inventory_Batches (Id),
        CONSTRAINT FK_Inventory_QuarantineHolds_Item FOREIGN KEY (ItemId) REFERENCES MasterData_Items (Id),
        CONSTRAINT FK_Inventory_QuarantineHolds_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Setting_Warehouses (Id)
    );
END
