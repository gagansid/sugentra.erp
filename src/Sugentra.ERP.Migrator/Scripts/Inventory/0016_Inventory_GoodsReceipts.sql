-- Inventory_GoodsReceipts: first-ever entry point for stock into the system (see docs/modules/inventory.md).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_GoodsReceipts')
BEGIN
    CREATE TABLE Inventory_GoodsReceipts
    (
        Id             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_GoodsReceipts PRIMARY KEY,
        -- Generated via DocumentNumberGeneratorService.GetNextNumberAsync("GoodsReceipt"), never hand-entered.
        ReceiptNumber  NVARCHAR(50)   NOT NULL,
        WarehouseId    BIGINT         NOT NULL,
        VendorReference NVARCHAR(200) NULL,
        ReceiptDate    DATETIME2      NOT NULL,
        Status         NVARCHAR(20)   NOT NULL CONSTRAINT DF_Inventory_GoodsReceipts_Status DEFAULT ('Draft'),
        Notes          NVARCHAR(500)  NULL,

        CreatedAt      DATETIME2      NOT NULL CONSTRAINT DF_Inventory_GoodsReceipts_CreatedAt DEFAULT (GETDATE()),
        CreatedBy      BIGINT         NULL,
        UpdatedAt      DATETIME2      NULL,
        UpdatedBy      BIGINT         NULL,
        IsDeleted      BIT            NOT NULL CONSTRAINT DF_Inventory_GoodsReceipts_IsDeleted DEFAULT (0),
        DeletedAt      DATETIME2      NULL,
        DeletedBy      BIGINT         NULL,

        CONSTRAINT UQ_Inventory_GoodsReceipts_ReceiptNumber UNIQUE (ReceiptNumber),
        CONSTRAINT CK_Inventory_GoodsReceipts_Status CHECK (Status IN ('Draft', 'Posted')),
        CONSTRAINT FK_Inventory_GoodsReceipts_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Setting_Warehouses (Id)
    );
END
GO
