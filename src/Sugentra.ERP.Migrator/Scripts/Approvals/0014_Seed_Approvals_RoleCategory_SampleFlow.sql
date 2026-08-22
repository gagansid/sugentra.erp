-- Authorizes the sample GoodsReceipt flow's roles (see 0010_Seed_Sample_ApprovalFlow.sql) so it still
-- validates once ApprovalFlowUseCase enforces Approval_RoleCategories on save/update.
INSERT INTO Approval_RoleCategories (RoleId, DocumentType)
SELECT r.Id, 'GoodsReceipt'
FROM Identity_Roles r
WHERE r.Name IN ('Manager', 'Admin', 'SuperAdmin')
  AND NOT EXISTS (
      SELECT 1 FROM Approval_RoleCategories rc WHERE rc.RoleId = r.Id AND rc.DocumentType = 'GoodsReceipt'
  );
GO
