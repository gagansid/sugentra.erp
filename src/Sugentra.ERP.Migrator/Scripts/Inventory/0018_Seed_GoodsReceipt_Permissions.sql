-- GoodsReceipt permissions (full CRUD), mirrors the pattern from 0011_Seed_Inventory_Permissions.sql.
DECLARE @Perms TABLE (Code NVARCHAR(100), Description NVARCHAR(300));
INSERT INTO @Perms (Code, Description) VALUES
    ('GoodsReceipt_Create', 'Create goods receipts'),
    ('GoodsReceipt_Edit',   'Edit/post goods receipts'),
    ('GoodsReceipt_Delete', 'Delete goods receipts'),
    ('GoodsReceipt_Report', 'Run goods receipt reports'),
    ('GoodsReceipt_View',   'View goods receipts');

INSERT INTO Identity_Permissions (Code, Module, ModuleId, Description)
SELECT p.Code, 'Inventory', m.Id, p.Description
FROM @Perms p
JOIN Setting_Modules m ON m.Code = 'Inventory'
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
