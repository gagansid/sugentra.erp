-- Stock Opname now routes through the Approvals engine (Draft -> WaitingApproval -> Completed) instead
-- of a manual Submit+Approve pair, mirroring GoodsReceipt/StockMutation.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Inventory_StockOpnames') AND name = 'CurrentApprovalLevel')
BEGIN
    ALTER TABLE Inventory_StockOpnames ADD CurrentApprovalLevel NVARCHAR(100) NULL;
END
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Inventory_StockOpnames_Status')
BEGIN
    ALTER TABLE Inventory_StockOpnames DROP CONSTRAINT CK_Inventory_StockOpnames_Status;
END
GO

UPDATE Inventory_StockOpnames SET Status = 'WaitingApproval' WHERE Status = 'Submitted';
UPDATE Inventory_StockOpnames SET Status = 'Completed' WHERE Status = 'Approved';
GO

ALTER TABLE Inventory_StockOpnames
    ADD CONSTRAINT CK_Inventory_StockOpnames_Status CHECK (Status IN ('Draft', 'WaitingApproval', 'Completed'));
GO
