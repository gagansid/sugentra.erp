-- Fixes 0005_Seed_Procurement_Permissions.sql, which filtered on role name 'SuperAdmin'
-- (no space) but the actual seeded role is named 'Super Admin', so it never got granted.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name = 'Super Admin'
  AND p.Code IN (
      'PurchaseRequisition_Create', 'PurchaseRequisition_Edit', 'PurchaseRequisition_Delete',
      'PurchaseRequisition_Approve', 'PurchaseRequisition_Report', 'PurchaseRequisition_View',
      'PurchaseOrder_Create', 'PurchaseOrder_Edit', 'PurchaseOrder_Delete',
      'PurchaseOrder_Approve', 'PurchaseOrder_Report', 'PurchaseOrder_View'
  )
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
GO
