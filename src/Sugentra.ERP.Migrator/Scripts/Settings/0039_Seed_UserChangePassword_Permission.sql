-- Seed: User_ChangePassword permission for admin-driven password reset (Users/Detail "Change Password" action).
-- Lives under Scripts/Settings (not Identity) because it needs Setting_Modules for the ModuleId lookup,
-- and ALL Identity/ scripts run before ANY Settings/ scripts on a fresh install (ScriptModuleOrderComparer).
DECLARE @ModuleId BIGINT = (SELECT Id FROM Setting_Modules WHERE Code = 'Identity');

INSERT INTO Identity_Permissions (Code, Module, ModuleId, Description)
SELECT 'User_ChangePassword', 'Identity', @ModuleId, 'Reset another user''s password'
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = 'User_ChangePassword');

-- Grant to SuperAdmin + Admin only (same tier as User_Unlock - administrative action, not Manager/Staff/Viewer level).
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name IN ('SuperAdmin', 'Admin')
  AND p.Code = 'User_ChangePassword'
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
GO
