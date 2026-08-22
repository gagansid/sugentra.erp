-- Removes the sample/demo data seeded by 0014_Seed_Sample_Inventory_Data.sql (child tables first for FK safety).
DELETE FROM Inventory_StockLedgers;
GO
DELETE FROM Inventory_StockBalances;
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
DELETE FROM Setting_Warehouses WHERE Code IN ('WH-MAIN', 'WH-QC', 'WH-VDR');
GO
