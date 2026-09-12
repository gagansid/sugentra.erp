-- Procurement permissions: full CRUD + Approve for PurchaseRequisition/PurchaseOrder.
DECLARE @Perms TABLE (Code NVARCHAR(100), Description NVARCHAR(300));
INSERT INTO @Perms (Code, Description) VALUES
    ('PurchaseRequisition_Create',  'Create purchase requisitions'),
    ('PurchaseRequisition_Edit',    'Edit purchase requisitions'),
    ('PurchaseRequisition_Delete',  'Delete purchase requisitions'),
    ('PurchaseRequisition_Approve', 'Approve/reject purchase requisitions'),
    ('PurchaseRequisition_Report',  'Run purchase requisition reports'),
    ('PurchaseRequisition_View',    'View purchase requisitions'),

    ('PurchaseOrder_Create',  'Create purchase orders'),
    ('PurchaseOrder_Edit',    'Edit purchase orders'),
    ('PurchaseOrder_Delete',  'Delete purchase orders'),
    ('PurchaseOrder_Approve', 'Approve/reject purchase orders'),
    ('PurchaseOrder_Report',  'Run purchase order reports'),
    ('PurchaseOrder_View',    'View purchase orders');

INSERT INTO Identity_Permissions (Code, Module, ModuleId, Description)
SELECT p.Code, 'Procurement', m.Id, p.Description
FROM @Perms p
JOIN Setting_Modules m ON m.Code = 'Procurement'
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = p.Code);

-- SuperAdmin + Admin: full access.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
JOIN @Perms np ON np.Code = p.Code
WHERE r.Name IN ('SuperAdmin', 'Admin')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );

-- Manager/Staff/Viewer: view-only.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
JOIN @Perms np ON np.Code = p.Code
WHERE r.Name IN ('Manager', 'Staff', 'Viewer')
  AND p.Code LIKE '%\_View' ESCAPE '\'
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
GO
