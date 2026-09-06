-- 0025 fixed the CHECK constraint, but any StockMutation posted between 0024 and 0025 already has a
-- Pending Approval_Requests row (created successfully) while its own Status update failed and rolled
-- back to 'Draft' (SetWaitingApprovalLevelAsync threw on the now-fixed constraint). Resync those rows.
UPDATE sm
SET sm.Status = 'WaitingApproval',
    sm.CurrentApprovalLevel = rl.Name
FROM Inventory_StockMutations sm
JOIN Approval_Requests r ON r.DocumentType = 'StockMutation' AND r.DocumentId = sm.Id
    AND r.Status = 'Pending' AND r.IsDeleted = 0
JOIN Approval_RequestLevels rl ON rl.RequestId = r.Id AND rl.LevelNumber = r.CurrentLevelNumber
WHERE sm.Status = 'Draft' AND sm.IsDeleted = 0;
GO
