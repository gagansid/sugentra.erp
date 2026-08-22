-- Seed: grant permissions to the sample Admin/Manager/Staff/Viewer roles (SuperAdmin already has everything from 0009).
-- Simple RBAC baseline for Phase 1 testing — adjust per real business rules later.

-- Admin: full administrative access, same breadth as SuperAdmin but as a distinct, revocable role.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name = 'Admin'
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );

-- Manager: can view everything, manage users, but not roles/permissions/settings.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name = 'Manager'
  AND p.Code IN ('User_View', 'User_Create', 'User_Update', 'Role_View', 'Permission_View', 'Settings_View', 'AuditLog_View')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );

-- Staff: standard operational user, read-only on Identity/Settings.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name = 'Staff'
  AND p.Code IN ('User_View', 'Settings_View')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );

-- Viewer: read-only across every module in the catalog.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name = 'Viewer'
  AND p.Code IN ('User_View', 'Role_View', 'Permission_View', 'Settings_View', 'AuditLog_View')
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
