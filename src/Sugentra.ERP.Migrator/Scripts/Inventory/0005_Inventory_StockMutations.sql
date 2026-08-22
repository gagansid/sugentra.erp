-- Inventory_StockMutations: internal transfer between warehouses, or transfer to/from an external subcontractor (see docs/modules/inventory.md).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_StockMutations')
BEGIN
    CREATE TABLE Inventory_StockMutations
    (
        Id                     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_StockMutations PRIMARY KEY,
        -- Generated via DocumentNumberGeneratorService.GetNextNumberAsync("StockMutation"), never hand-entered.
        MutationNumber         NVARCHAR(50)   NOT NULL,
        MutationType           NVARCHAR(20)   NOT NULL,
        SourceWarehouseId      BIGINT         NOT NULL,
        -- Nullable when MutationType is ToVendor/FromVendor - destination/source is an external party (VendorReference), not a warehouse.
        DestinationWarehouseId BIGINT         NULL,
        VendorReference        NVARCHAR(200)  NULL,
        MutationDate           DATETIME2      NOT NULL,
        Status                 NVARCHAR(20)   NOT NULL CONSTRAINT DF_Inventory_StockMutations_Status DEFAULT ('Draft'),
        Notes                  NVARCHAR(500)  NULL,

        CreatedAt              DATETIME2      NOT NULL CONSTRAINT DF_Inventory_StockMutations_CreatedAt DEFAULT (GETDATE()),
        CreatedBy              BIGINT         NULL,
        UpdatedAt              DATETIME2      NULL,
        UpdatedBy              BIGINT         NULL,
        IsDeleted              BIT            NOT NULL CONSTRAINT DF_Inventory_StockMutations_IsDeleted DEFAULT (0),
        DeletedAt              DATETIME2      NULL,
        DeletedBy              BIGINT         NULL,

        CONSTRAINT UQ_Inventory_StockMutations_MutationNumber UNIQUE (MutationNumber),
        CONSTRAINT CK_Inventory_StockMutations_MutationType CHECK (MutationType IN ('Internal', 'ToVendor', 'FromVendor')),
        CONSTRAINT CK_Inventory_StockMutations_Status CHECK (Status IN ('Draft', 'Approved', 'Completed')),
        CONSTRAINT FK_Inventory_StockMutations_SourceWarehouse FOREIGN KEY (SourceWarehouseId) REFERENCES Setting_Warehouses (Id),
        CONSTRAINT FK_Inventory_StockMutations_DestinationWarehouse FOREIGN KEY (DestinationWarehouseId) REFERENCES Setting_Warehouses (Id)
    );
END
