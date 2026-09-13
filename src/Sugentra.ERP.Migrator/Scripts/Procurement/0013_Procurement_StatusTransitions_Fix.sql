-- Fixes 0011_Procurement_StatusTransitions.sql: that seed mixed approval-axis Status values (Draft/
-- PendingApproval, and the approval-flow uses 'WaitingApproval' anyway, not 'PendingApproval') into what
-- should be a pure LifecycleStatus transition table. LifecycleStatus only exists once Status == 'Approved'
-- and starts at 'Open' (see 0012_Procurement_LifecycleStatus.sql / docs/modules/procurement.md) - it never
-- transitions from 'Draft'/'PendingApproval'/'Approved' itself, so those rows don't belong here.
DELETE FROM Procurement_StatusTransitions
WHERE CategoryGroup = 'PurchaseOrder' AND FromStatus IN ('Draft', 'PendingApproval');

UPDATE Procurement_StatusTransitions
SET FromStatus = 'Open'
WHERE CategoryGroup IN ('PurchaseOrder', 'PurchaseRequisition') AND FromStatus = 'Approved';
GO
