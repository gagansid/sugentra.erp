-- ErrorLog_Edit: set condition (NotSolved/Solved) + remarks, and send the notification email.
-- Same grant pattern as the rest of 0036: SuperAdmin/Admin get it, Manager/Staff/Viewer stay view-only.
IF NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = 'ErrorLog_Edit')
BEGIN
    INSERT INTO Identity_Permissions (Code, Module, ModuleId, Description)
    SELECT 'ErrorLog_Edit', 'AuditLog', m.Id, 'Set error log condition, remarks, and send notification email'
    FROM Setting_Modules m WHERE m.Code = 'AuditLog';
END
GO

INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE p.Code = 'ErrorLog_Edit'
  AND r.Name IN ('SuperAdmin', 'Admin')
  AND NOT EXISTS (SELECT 1 FROM Identity_RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id);
GO
