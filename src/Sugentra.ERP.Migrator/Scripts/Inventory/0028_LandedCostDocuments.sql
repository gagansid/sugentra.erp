-- Landed cost is now a header+lines document posted against a GoodsReceipt, auto-allocating cost to each
-- received batch (see docs/modules/inventory.md). Inventory_LandedCostAllocations becomes computed output.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_LandedCostDocuments')
BEGIN
    CREATE TABLE Inventory_LandedCostDocuments
    (
        Id               BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_LandedCostDocuments PRIMARY KEY,
        DocumentNumber   NVARCHAR(50)   NOT NULL,
        GoodsReceiptId   BIGINT         NOT NULL,
        AllocationMethod NVARCHAR(20)   NOT NULL CONSTRAINT DF_Inventory_LandedCostDocuments_AllocationMethod DEFAULT ('ByValue'),
        Status           NVARCHAR(20)   NOT NULL CONSTRAINT DF_Inventory_LandedCostDocuments_Status DEFAULT ('Draft'),
        Notes            NVARCHAR(500)  NULL,
        CreatedAt        DATETIME2      NOT NULL CONSTRAINT DF_Inventory_LandedCostDocuments_CreatedAt DEFAULT (GETDATE()),
        CreatedBy        BIGINT         NULL,
        UpdatedAt        DATETIME2      NULL,
        UpdatedBy        BIGINT         NULL,
        IsDeleted        BIT            NOT NULL CONSTRAINT DF_Inventory_LandedCostDocuments_IsDeleted DEFAULT (0),
        DeletedAt        DATETIME2      NULL,
        DeletedBy        BIGINT         NULL,
        CONSTRAINT FK_Inventory_LandedCostDocuments_GoodsReceipt FOREIGN KEY (GoodsReceiptId) REFERENCES Inventory_GoodsReceipts (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_LandedCostDocumentLines')
BEGIN
    CREATE TABLE Inventory_LandedCostDocumentLines
    (
        Id                    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_LandedCostDocumentLines PRIMARY KEY,
        LandedCostDocumentId  BIGINT         NOT NULL,
        CostType              NVARCHAR(20)   NOT NULL,
        Amount                DECIMAL(18,2)  NOT NULL,
        CurrencyId            BIGINT         NOT NULL,
        Notes                 NVARCHAR(500)  NULL,
        CreatedAt             DATETIME2      NOT NULL CONSTRAINT DF_Inventory_LandedCostDocumentLines_CreatedAt DEFAULT (GETDATE()),
        CreatedBy             BIGINT         NULL,
        UpdatedAt             DATETIME2      NULL,
        UpdatedBy             BIGINT         NULL,
        IsDeleted             BIT            NOT NULL CONSTRAINT DF_Inventory_LandedCostDocumentLines_IsDeleted DEFAULT (0),
        DeletedAt             DATETIME2      NULL,
        DeletedBy             BIGINT         NULL,
        CONSTRAINT FK_Inventory_LandedCostDocumentLines_Document FOREIGN KEY (LandedCostDocumentId) REFERENCES Inventory_LandedCostDocuments (Id),
        CONSTRAINT FK_Inventory_LandedCostDocumentLines_Currency FOREIGN KEY (CurrencyId) REFERENCES Setting_Currencies (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Inventory_LandedCostAllocations') AND name = 'LandedCostDocumentId')
BEGIN
    ALTER TABLE Inventory_LandedCostAllocations ADD LandedCostDocumentId BIGINT NULL
        CONSTRAINT FK_Inventory_LandedCostAllocations_Document FOREIGN KEY REFERENCES Inventory_LandedCostDocuments (Id);
END
