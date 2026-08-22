-- Inventory_GoodsReceiptLines: line items for a goods receipt, each posting a Receipt stock ledger entry.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_GoodsReceiptLines')
BEGIN
    CREATE TABLE Inventory_GoodsReceiptLines
    (
        Id         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_GoodsReceiptLines PRIMARY KEY,
        ReceiptId  BIGINT         NOT NULL,
        ItemId     BIGINT         NOT NULL,
        BatchId    BIGINT         NOT NULL,
        Quantity   DECIMAL(18,4)  NOT NULL,
        UnitCost   DECIMAL(18,2)  NOT NULL,

        CreatedAt  DATETIME2      NOT NULL CONSTRAINT DF_Inventory_GoodsReceiptLines_CreatedAt DEFAULT (GETDATE()),
        CreatedBy  BIGINT         NULL,
        UpdatedAt  DATETIME2      NULL,
        UpdatedBy  BIGINT         NULL,
        IsDeleted  BIT            NOT NULL CONSTRAINT DF_Inventory_GoodsReceiptLines_IsDeleted DEFAULT (0),
        DeletedAt  DATETIME2      NULL,
        DeletedBy  BIGINT         NULL,

        CONSTRAINT FK_Inventory_GoodsReceiptLines_Receipt FOREIGN KEY (ReceiptId) REFERENCES Inventory_GoodsReceipts (Id),
        CONSTRAINT FK_Inventory_GoodsReceiptLines_Batch FOREIGN KEY (BatchId) REFERENCES Inventory_Batches (Id)
    );
END
GO
