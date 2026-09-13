-- Full reset of Inventory transactional data (child tables first for FK safety) plus identity reseed,
-- so all document numbers restart at 1. Master data (Items/Warehouses/UoM) is untouched.
DELETE FROM Inventory_LandedCostAllocations;
GO
DELETE FROM Inventory_LandedCostDocumentLines;
GO
DELETE FROM Inventory_LandedCostDocuments;
GO
DELETE FROM Inventory_StockLedgers;
GO
DELETE FROM Inventory_StockBalances;
GO
DELETE FROM Inventory_GoodsReceiptLines;
GO
DELETE FROM Inventory_GoodsReceipts;
GO
DELETE FROM Inventory_StockOpnameLines;
GO
DELETE FROM Inventory_StockOpnames;
GO
DELETE FROM Inventory_StockMutationLines;
GO
DELETE FROM Inventory_StockMutations;
GO
DELETE FROM Inventory_QuarantineHolds;
GO
DELETE FROM Inventory_Batches;
GO

DBCC CHECKIDENT ('Inventory_LandedCostAllocations', RESEED, 0);
DBCC CHECKIDENT ('Inventory_LandedCostDocumentLines', RESEED, 0);
DBCC CHECKIDENT ('Inventory_LandedCostDocuments', RESEED, 0);
DBCC CHECKIDENT ('Inventory_StockLedgers', RESEED, 0);
DBCC CHECKIDENT ('Inventory_StockBalances', RESEED, 0);
DBCC CHECKIDENT ('Inventory_GoodsReceiptLines', RESEED, 0);
DBCC CHECKIDENT ('Inventory_GoodsReceipts', RESEED, 0);
DBCC CHECKIDENT ('Inventory_StockOpnameLines', RESEED, 0);
DBCC CHECKIDENT ('Inventory_StockOpnames', RESEED, 0);
DBCC CHECKIDENT ('Inventory_StockMutationLines', RESEED, 0);
DBCC CHECKIDENT ('Inventory_StockMutations', RESEED, 0);
DBCC CHECKIDENT ('Inventory_QuarantineHolds', RESEED, 0);
DBCC CHECKIDENT ('Inventory_Batches', RESEED, 0);
GO
