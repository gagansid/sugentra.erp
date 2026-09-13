-- Adds the second, independent status axis described in docs/modules/procurement.md ("Planned: Post-Approval
-- Correction Workflow"): LifecycleStatus tracks fulfillment/lifecycle progress (Open/PartiallyReceived/
-- FullyReceived/Cancelled/Closed/Superseded for PO, Open/Closed for PR), separate from the existing Status
-- column which continues to track the approval workflow (Draft/WaitingApproval/Approved/Rejected).
-- Backfilled from current Status values so existing rows immediately have a sensible LifecycleStatus.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Procurement_PurchaseOrders') AND name = 'LifecycleStatus')
BEGIN
    ALTER TABLE Procurement_PurchaseOrders ADD LifecycleStatus NVARCHAR(20) NULL;

    -- EXEC() defers parsing/name resolution to runtime, since the column added above isn't visible to the
    -- rest of this batch's compile-time column resolution otherwise (no GO allowed inside BEGIN/END here).
    EXEC('UPDATE Procurement_PurchaseOrders
        SET LifecycleStatus = CASE Status
            WHEN ''Approved'' THEN ''Open''
            WHEN ''PartiallyReceived'' THEN ''PartiallyReceived''
            WHEN ''FullyReceived'' THEN ''FullyReceived''
            WHEN ''Closed'' THEN ''Closed''
            ELSE NULL
        END');

    EXEC('ALTER TABLE Procurement_PurchaseOrders ADD CONSTRAINT CK_Procurement_PurchaseOrders_LifecycleStatus
        CHECK (LifecycleStatus IN (''Open'', ''PartiallyReceived'', ''FullyReceived'', ''Cancelled'', ''Closed'', ''Superseded''))');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Procurement_PurchaseRequisitions') AND name = 'LifecycleStatus')
BEGIN
    ALTER TABLE Procurement_PurchaseRequisitions ADD LifecycleStatus NVARCHAR(20) NULL;

    EXEC('UPDATE Procurement_PurchaseRequisitions
        SET LifecycleStatus = CASE Status
            WHEN ''Approved'' THEN ''Open''
            WHEN ''Closed'' THEN ''Closed''
            ELSE NULL
        END');

    EXEC('ALTER TABLE Procurement_PurchaseRequisitions ADD CONSTRAINT CK_Procurement_PurchaseRequisitions_LifecycleStatus
        CHECK (LifecycleStatus IN (''Open'', ''Closed''))');
END
GO
