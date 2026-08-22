-- Status is now a fixed code (Draft | WaitingApproval | Posted); the human-readable "Waiting Approval -
-- <level name>" text is composed in the UI from CurrentApprovalLevel instead of being stored verbatim
-- (the old free-text format didn't fit the column and had no upper bound on level-name length).
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Inventory_GoodsReceipts') AND name = 'CurrentApprovalLevel')
BEGIN
    ALTER TABLE Inventory_GoodsReceipts ADD CurrentApprovalLevel NVARCHAR(100) NULL;
END
GO

-- Backfill any rows already in the old "Waiting Approval - <level>" free-text format, if present.
UPDATE Inventory_GoodsReceipts
SET CurrentApprovalLevel = SUBSTRING(Status, LEN('Waiting Approval - ') + 1, 100),
    Status = 'WaitingApproval'
WHERE Status LIKE 'Waiting Approval - %';
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Inventory_GoodsReceipts_Status')
BEGIN
    ALTER TABLE Inventory_GoodsReceipts DROP CONSTRAINT CK_Inventory_GoodsReceipts_Status;
END
GO

ALTER TABLE Inventory_GoodsReceipts
    ADD CONSTRAINT CK_Inventory_GoodsReceipts_Status CHECK (Status IN ('Draft', 'WaitingApproval', 'Posted'));
GO
