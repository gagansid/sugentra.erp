-- Inventory_StockOpnames: physical count reconciliation header (see docs/modules/inventory.md).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_StockOpnames')
BEGIN
    CREATE TABLE Inventory_StockOpnames
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_StockOpnames PRIMARY KEY,
        -- Generated via DocumentNumberGeneratorService.GetNextNumberAsync("StockOpname"), never hand-entered.
        OpnameNumber  NVARCHAR(50)   NOT NULL,
        WarehouseId   BIGINT         NOT NULL,
        OpnameDate    DATETIME2      NOT NULL,
        Status        NVARCHAR(20)   NOT NULL CONSTRAINT DF_Inventory_StockOpnames_Status DEFAULT ('Draft'),
        Notes         NVARCHAR(500)  NULL,

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Inventory_StockOpnames_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Inventory_StockOpnames_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT UQ_Inventory_StockOpnames_OpnameNumber UNIQUE (OpnameNumber),
        CONSTRAINT CK_Inventory_StockOpnames_Status CHECK (Status IN ('Draft', 'Submitted', 'Approved')),
        CONSTRAINT FK_Inventory_StockOpnames_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Setting_Warehouses (Id)
    );
END
