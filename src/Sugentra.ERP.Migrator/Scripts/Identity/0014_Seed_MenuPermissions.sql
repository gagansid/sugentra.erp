-- Seed: Menu_View/Menu_Manage permission codes + grants for the sample roles (same policy as Module_View/Manage).
-- Module = 'SystemAdministration' so ModuleQuery.GetActiveForPermissionCodesAsync matches Setting_Modules.Code and the dashboard tile appears.
INSERT INTO Identity_Permissions (Code, Module, Description)
SELECT v.Code, v.Module, v.Description
FROM (VALUES
    ('Menu_View',   'SystemAdministration', 'View menu catalog'),
    ('Menu_Manage', 'SystemAdministration', 'Create/update/deactivate menus')
) AS v(Code, Module, Description)
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = v.Code);

-- SuperAdmin: full access.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name = 'SuperAdmin'
  AND p.Code IN ('Menu_View', 'Menu_Manage')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );

-- Admin: full manage access.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name = 'Admin'
  AND p.Code IN ('Menu_View', 'Menu_Manage')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );

-- Manager/Viewer: read-only.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name IN ('Manager', 'Viewer')
  AND p.Code = 'Menu_View'
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
GO
