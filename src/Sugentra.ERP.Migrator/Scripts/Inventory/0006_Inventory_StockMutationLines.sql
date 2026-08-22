-- Inventory_StockMutationLines: item/qty lines for a Stock Mutation header (see docs/modules/inventory.md).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_StockMutationLines')
BEGIN
    CREATE TABLE Inventory_StockMutationLines
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_StockMutationLines PRIMARY KEY,
        MutationId  BIGINT         NOT NULL,
        ItemId      BIGINT         NOT NULL,
        BatchId     BIGINT         NULL,
        Quantity    DECIMAL(18,3)  NOT NULL,

        CreatedAt   DATETIME2      NOT NULL CONSTRAINT DF_Inventory_StockMutationLines_CreatedAt DEFAULT (GETDATE()),
        CreatedBy   BIGINT         NULL,
        UpdatedAt   DATETIME2      NULL,
        UpdatedBy   BIGINT         NULL,
        IsDeleted   BIT            NOT NULL CONSTRAINT DF_Inventory_StockMutationLines_IsDeleted DEFAULT (0),
        DeletedAt   DATETIME2      NULL,
        DeletedBy   BIGINT         NULL,

        CONSTRAINT FK_Inventory_StockMutationLines_Mutation FOREIGN KEY (MutationId) REFERENCES Inventory_StockMutations (Id),
        CONSTRAINT FK_Inventory_StockMutationLines_Item FOREIGN KEY (ItemId) REFERENCES MasterData_Items (Id),
        CONSTRAINT FK_Inventory_StockMutationLines_Batch FOREIGN KEY (BatchId) REFERENCES Inventory_Batches (Id)
    );

    CREATE INDEX IX_Inventory_StockMutationLines_Mutation ON Inventory_StockMutationLines (MutationId);
END
