-- ErrorLog_SendEmail: send the notification email. Split out from ErrorLog_Edit so anyone who can view
-- error logs can send the notification, while editing condition/remarks stays restricted to ErrorLog_Edit (IT).
IF NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = 'ErrorLog_SendEmail')
BEGIN
    INSERT INTO Identity_Permissions (Code, Module, ModuleId, Description)
    SELECT 'ErrorLog_SendEmail', 'AuditLog', m.Id, 'Send error log notification email'
    FROM Setting_Modules m WHERE m.Code = 'AuditLog';
END
GO

-- Same grant pattern as ErrorLog_View: every role gets it.
INSERT INTO Identity_RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, NULL
FROM Identity_Roles r
CROSS JOIN Identity_Permissions p
WHERE p.Code = 'ErrorLog_SendEmail'
  AND NOT EXISTS (SELECT 1 FROM Identity_RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id);
GO

-- ErrorLog_Edit no longer covers sending email - keep the description in sync without touching the original insert.
UPDATE Identity_Permissions SET Description = 'Set error log condition and remarks' WHERE Code = 'ErrorLog_Edit';
GO
