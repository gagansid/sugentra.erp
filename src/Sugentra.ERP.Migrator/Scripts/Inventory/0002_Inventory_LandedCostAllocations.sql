-- Inventory_LandedCostAllocations: freight/insurance/handling/duty costs allocated to a received batch (see docs/modules/inventory.md).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory_LandedCostAllocations')
BEGIN
    CREATE TABLE Inventory_LandedCostAllocations
    (
        Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory_LandedCostAllocations PRIMARY KEY,
        BatchId      BIGINT         NOT NULL,
        CostType     NVARCHAR(20)   NOT NULL,
        Amount       DECIMAL(18,2)  NOT NULL,
        CurrencyId   BIGINT         NOT NULL,
        Notes        NVARCHAR(500)  NULL,

        CreatedAt    DATETIME2      NOT NULL CONSTRAINT DF_Inventory_LandedCostAllocations_CreatedAt DEFAULT (GETDATE()),
        CreatedBy    BIGINT         NULL,
        UpdatedAt    DATETIME2      NULL,
        UpdatedBy    BIGINT         NULL,
        IsDeleted    BIT            NOT NULL CONSTRAINT DF_Inventory_LandedCostAllocations_IsDeleted DEFAULT (0),
        DeletedAt    DATETIME2      NULL,
        DeletedBy    BIGINT         NULL,

        CONSTRAINT CK_Inventory_LandedCostAllocations_CostType CHECK (CostType IN ('Freight', 'Insurance', 'Handling', 'Duty', 'Other')),
        CONSTRAINT FK_Inventory_LandedCostAllocations_Batch FOREIGN KEY (BatchId) REFERENCES Inventory_Batches (Id),
        CONSTRAINT FK_Inventory_LandedCostAllocations_Currency FOREIGN KEY (CurrencyId) REFERENCES Setting_Currencies (Id)
    );
END
