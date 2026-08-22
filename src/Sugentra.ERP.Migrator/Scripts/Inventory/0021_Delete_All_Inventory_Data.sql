-- Purge all Inventory transactional/master data currently in the DB (child tables first for FK safety),
-- so the module starts clean again before the Batch-delete stock-guard is added.
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
DELETE FROM Inventory_LandedCostAllocations;
GO
DELETE FROM Inventory_Batches;
GO
