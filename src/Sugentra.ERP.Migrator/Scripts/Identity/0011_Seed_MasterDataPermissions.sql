-- Seed: MasterData_View/MasterData_Manage permission codes + grants for the sample roles (same policy as Settings_View/Manage).
INSERT INTO Identity_Permissions (Code, Module, Description)
SELECT v.Code, v.Module, v.Description
FROM (VALUES
    ('MasterData_View',   'MasterData', 'View business partners, items, BOM, price lists'),
    ('MasterData_Manage', 'MasterData', 'Manage business partners, items, BOM, price lists')
) AS v(Code, Module, Description)
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = v.Code);

-- SuperAdmin: every permission in the catalog (existing 0009 CROSS JOIN grant already covers new rows on re-run, but
-- SuperAdmin was already granted before these two existed, so grant explicitly here too).
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name = 'SuperAdmin'
  AND p.Code IN ('MasterData_View', 'MasterData_Manage')
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
  AND p.Code IN ('MasterData_View', 'MasterData_Manage')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );

-- Manager: view + manage (day-to-day master data upkeep).
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name = 'Manager'
  AND p.Code IN ('MasterData_View', 'MasterData_Manage')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );

-- Staff and Viewer: view only.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name IN ('Staff', 'Viewer')
  AND p.Code = 'MasterData_View'
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
