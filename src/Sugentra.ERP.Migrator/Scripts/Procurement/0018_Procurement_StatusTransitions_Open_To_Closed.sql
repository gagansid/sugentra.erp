-- Adds PurchaseOrder Open -> Closed (close a PO before any receipt was posted, releasing 100% of its
-- remainder back to the source PR). PartiallyReceived -> Closed already existed from 0011/0013.
IF NOT EXISTS (SELECT 1 FROM Procurement_StatusTransitions WHERE CategoryGroup = 'PurchaseOrder' AND FromStatus = 'Open' AND ToStatus = 'Closed')
BEGIN
    INSERT INTO Procurement_StatusTransitions (CategoryGroup, FromStatus, ToStatus, ConditionDescription, IsSystemTriggered)
    VALUES ('PurchaseOrder', 'Open', 'Closed', 'Closed by Admin/Manager before any Goods Receipt was posted', 0);
END
GO
