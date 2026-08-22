-- Error Log audit trail: read-only permission (ErrorLog_View) + sidebar menu entry alongside Audit Log,
-- since both are system-monitoring/troubleshooting screens under the 'AuditLog' module group.
IF NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = 'ErrorLog_View')
BEGIN
    INSERT INTO Identity_Permissions (Code, Module, ModuleId, Description)
    SELECT 'ErrorLog_View', 'AuditLog', m.Id, 'View error logs'
    FROM Setting_Modules m WHERE m.Code = 'AuditLog';
END
GO

-- SuperAdmin/Admin/Manager/Staff/Viewer: view-only, same grant pattern as AuditLog_View.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE p.Code = 'ErrorLog_View'
  AND NOT EXISTS (SELECT 1 FROM Identity_RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id);
GO

DECLARE @ModuleId BIGINT = (SELECT Id FROM Setting_Modules WHERE Code = 'AuditLog');

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT @ModuleId, NULL, 'Error Logs', 'ri-bug-line', 'ErrorLogs', 'Index', 'ErrorLog_View', 20
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_Menus
    WHERE ModuleId = @ModuleId AND Controller = 'ErrorLogs' AND Action = 'Index'
);
GO
