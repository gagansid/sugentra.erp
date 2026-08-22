-- Seed Setting_Menus with the Email Configuration entries under the System Administration group.
DECLARE @Rows TABLE (ModuleCode NVARCHAR(50), Name NVARCHAR(100), Icon NVARCHAR(50), Ctrl NVARCHAR(100), Act NVARCHAR(100), Perm NVARCHAR(100), SortOrder INT);
INSERT INTO @Rows (ModuleCode, Name, Icon, Ctrl, Act, Perm, SortOrder) VALUES
    ('SystemAdministration', 'Email Settings', 'ri-mail-settings-line', 'EmailSettings', 'Index', 'Settings_View', 30),
    ('SystemAdministration', 'Email Templates', 'ri-mail-open-line', 'EmailTemplates', 'Index', 'Settings_View', 40),
    ('SystemAdministration', 'Email History', 'ri-mail-check-line', 'EmailHistory', 'Index', 'Settings_View', 50);

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT mo.Id, NULL, r.Name, r.Icon, r.Ctrl, r.Act, r.Perm, r.SortOrder
FROM @Rows r
JOIN Setting_Modules mo ON mo.Code = r.ModuleCode
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_Menus me
    WHERE me.ModuleId = mo.Id AND me.Controller = r.Ctrl AND me.Action = r.Act
);
GO
