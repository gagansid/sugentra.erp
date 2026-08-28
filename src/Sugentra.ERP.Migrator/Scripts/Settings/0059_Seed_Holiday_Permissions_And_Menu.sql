-- Permissions + role grants + sidebar menu entry for the new Holidays calendar screen.
DECLARE @Perms TABLE (Code NVARCHAR(100), ModuleCode NVARCHAR(50), Description NVARCHAR(300));
INSERT INTO @Perms (Code, ModuleCode, Description) VALUES
    ('Holiday_Create', 'Settings', 'Create holidays'),
    ('Holiday_Edit',   'Settings', 'Edit holidays'),
    ('Holiday_Delete', 'Settings', 'Delete holidays'),
    ('Holiday_View',   'Settings', 'View holidays');

INSERT INTO Identity_Permissions (Code, Module, ModuleId, Description)
SELECT p.Code, p.ModuleCode, m.Id, p.Description
FROM @Perms p
JOIN Setting_Modules m ON m.Code = p.ModuleCode
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = p.Code);

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

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT mo.Id, NULL, 'Holidays', 'ri-calendar-event-line', 'Holidays', 'Index', 'Holiday_View', 35
FROM Setting_Modules mo
WHERE mo.Code = 'Settings'
  AND NOT EXISTS (
      SELECT 1 FROM Setting_Menus me
      WHERE me.ModuleId = mo.Id AND me.Controller = 'Holidays' AND me.Action = 'Index'
  );
GO
