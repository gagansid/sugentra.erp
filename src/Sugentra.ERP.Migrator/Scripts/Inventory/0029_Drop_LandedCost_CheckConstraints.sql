-- Per convention going forward: enum-like Status/Type columns are validated in the application layer only
-- (entity/UseCase), not via DB CHECK constraints -- avoids a migration every time a new status/type value
-- is added (see 0022/0025/0027 for the churn this caused on GoodsReceipts/StockMutations/StockOpnames).
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Inventory_LandedCostDocuments_Status')
BEGIN
    ALTER TABLE Inventory_LandedCostDocuments DROP CONSTRAINT CK_Inventory_LandedCostDocuments_Status;
END
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Inventory_LandedCostDocuments_AllocationMethod')
BEGIN
    ALTER TABLE Inventory_LandedCostDocuments DROP CONSTRAINT CK_Inventory_LandedCostDocuments_AllocationMethod;
END
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Inventory_LandedCostDocumentLines_CostType')
BEGIN
    ALTER TABLE Inventory_LandedCostDocumentLines DROP CONSTRAINT CK_Inventory_LandedCostDocumentLines_CostType;
END
GO
