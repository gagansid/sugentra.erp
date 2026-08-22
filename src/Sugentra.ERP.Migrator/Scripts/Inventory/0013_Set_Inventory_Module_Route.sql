-- Inventory module now has a working workspace (Batches, Stock Mutations/Opnames, Quarantine Holds, Stock Balances/Ledgers).
UPDATE Setting_Modules SET Route = '/Workspace/Inventory' WHERE Code = 'Inventory' AND Route IS NULL;
GO
