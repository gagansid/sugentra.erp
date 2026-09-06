-- 0024 backfilled Status='WaitingApproval' but never widened the CHECK constraint, so the app hit
-- "CK_Inventory_StockMutations_Status" whenever it tried to actually set that status at runtime.
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Inventory_StockMutations_Status')
BEGIN
    ALTER TABLE Inventory_StockMutations DROP CONSTRAINT CK_Inventory_StockMutations_Status;
END
GO

ALTER TABLE Inventory_StockMutations
    ADD CONSTRAINT CK_Inventory_StockMutations_Status CHECK (Status IN ('Draft', 'WaitingApproval', 'Completed'));
GO
