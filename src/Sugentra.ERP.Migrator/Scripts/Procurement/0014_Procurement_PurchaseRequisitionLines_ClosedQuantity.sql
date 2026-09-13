-- Per-line "close remainder" flag/qty for PR Cancel/Close (see docs/modules/procurement.md).
-- Not a new versioning mechanic - just tracks how much of a line's un-ordered qty was force-closed.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Procurement_PurchaseRequisitionLines') AND name = 'ClosedQuantity')
BEGIN
    ALTER TABLE Procurement_PurchaseRequisitionLines ADD ClosedQuantity DECIMAL(18, 4) NOT NULL CONSTRAINT DF_Procurement_PurchaseRequisitionLines_ClosedQuantity DEFAULT (0);
END
GO
