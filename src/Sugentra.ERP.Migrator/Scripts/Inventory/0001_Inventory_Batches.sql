-- Inventory_Batches: traceability + legality unit for raw material/finished goods (see docs/modules/inventory.md).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_Batches')
BEGIN
    CREATE TABLE Inventory_Batches
    (
        Id                     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_Batches PRIMARY KEY,
        Code                   NVARCHAR(50)   NOT NULL,
        ItemId                 BIGINT         NOT NULL,
        Grade                  NVARCHAR(50)   NULL,
        WarehouseId            BIGINT         NOT NULL,
        ReceivedDate           DATETIME2      NOT NULL,
        LegalityDocumentType   NVARCHAR(30)   NULL,
        LegalityDocumentNumber NVARCHAR(100)  NULL,
        LegalityDocumentUrl    NVARCHAR(500)  NULL,
        SourceReference        NVARCHAR(100)  NULL,

        CreatedAt              DATETIME2      NOT NULL CONSTRAINT DF_Inventory_Batches_CreatedAt DEFAULT (GETDATE()),
        CreatedBy              BIGINT         NULL,
        UpdatedAt              DATETIME2      NULL,
        UpdatedBy              BIGINT         NULL,
        IsDeleted              BIT            NOT NULL CONSTRAINT DF_Inventory_Batches_IsDeleted DEFAULT (0),
        DeletedAt              DATETIME2      NULL,
        DeletedBy              BIGINT         NULL,

        CONSTRAINT UQ_Inventory_Batches_Code UNIQUE (Code),
        CONSTRAINT CK_Inventory_Batches_LegalityDocumentType CHECK (LegalityDocumentType IS NULL OR LegalityDocumentType IN ('SVLK', 'FSC', 'Other')),
        CONSTRAINT FK_Inventory_Batches_Item FOREIGN KEY (ItemId) REFERENCES MasterData_Items (Id),
        CONSTRAINT FK_Inventory_Batches_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Setting_Warehouses (Id)
    );
END
