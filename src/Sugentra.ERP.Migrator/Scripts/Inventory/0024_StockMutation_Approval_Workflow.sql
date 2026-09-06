-- Stock Mutation now routes through the Approvals engine (Draft -> WaitingApproval -> Completed) instead
-- of a direct Approve button, mirroring GoodsReceipt. CurrentApprovalLevel mirrors Inventory_GoodsReceipts.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Inventory_StockMutations') AND name = 'CurrentApprovalLevel')
BEGIN
    ALTER TABLE Inventory_StockMutations ADD CurrentApprovalLevel NVARCHAR(100) NULL;
END
GO

UPDATE Inventory_StockMutations SET Status = 'WaitingApproval' WHERE Status = 'Approved';
GO
