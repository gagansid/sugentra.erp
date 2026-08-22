-- Seed: User_Unlock permission for admin-driven account unlock (see AGENTS.md account lockout enforcement).
INSERT INTO Identity_Permissions (Code, Module, Description)
SELECT v.Code, v.Module, v.Description
FROM (VALUES
    ('User_Unlock', 'Identity', 'Unlock a locked-out user account')
) AS v(Code, Module, Description)
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = v.Code);

-- Grant to SuperAdmin + Admin only (unlock is an administrative action, not Manager/Staff/Viewer level).
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE r.Name IN ('SuperAdmin', 'Admin')
  AND p.Code = 'User_Unlock'
  AND NOT EXISTS (
      SELECT 1 FROM Identity_RolePermissions rp
      WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
  );
