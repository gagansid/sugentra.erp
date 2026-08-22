-- Replaces the coarse Settings_View/Settings_Manage pair (and Module_Manage/Menu_Manage) with per-entity
-- Create/Edit/Delete/Report/View permissions, so each Settings/System Administration screen can be granted
-- independently. Old codes are left in place (unreferenced by any controller from now on) rather than deleted,
-- to avoid cascading through Identity_RolePermissions/Identity_UserPermissions rows that already reference them.
-- Report has no endpoint yet - seeded now so a future reporting feature can slot straight into the existing scheme.
DECLARE @Perms TABLE (Code NVARCHAR(100), ModuleCode NVARCHAR(50), Description NVARCHAR(300));
INSERT INTO @Perms (Code, ModuleCode, Description) VALUES
    ('ApprovalMatrix_Create', 'Settings', 'Create approval matrix entries'),
    ('ApprovalMatrix_Edit',   'Settings', 'Edit approval matrix entries'),
    ('ApprovalMatrix_Delete', 'Settings', 'Delete approval matrix entries'),
    ('ApprovalMatrix_Report', 'Settings', 'Run approval matrix reports'),
    ('ApprovalMatrix_View',   'Settings', 'View approval matrix entries'),

    ('CompanyProfile_Create', 'Settings', 'Create company profile'),
    ('CompanyProfile_Edit',   'Settings', 'Edit company profile'),
    ('CompanyProfile_Delete', 'Settings', 'Delete company profile'),
    ('CompanyProfile_Report', 'Settings', 'Run company profile reports'),
    ('CompanyProfile_View',   'Settings', 'View company profile'),

    ('Currency_Create', 'Settings', 'Create currencies'),
    ('Currency_Edit',   'Settings', 'Edit currencies'),
    ('Currency_Delete', 'Settings', 'Delete currencies'),
    ('Currency_Report', 'Settings', 'Run currency reports'),
    ('Currency_View',   'Settings', 'View currencies'),

    ('DocumentNumbering_Create', 'Settings', 'Create document numbering configs'),
    ('DocumentNumbering_Edit',   'Settings', 'Edit document numbering configs'),
    ('DocumentNumbering_Delete', 'Settings', 'Delete document numbering configs'),
    ('DocumentNumbering_Report', 'Settings', 'Run document numbering reports'),
    ('DocumentNumbering_View',   'Settings', 'View document numbering configs'),

    ('EmailHistory_Create', 'Settings', 'Create email history entries'),
    ('EmailHistory_Edit',   'Settings', 'Edit email history entries'),
    ('EmailHistory_Delete', 'Settings', 'Delete email history entries'),
    ('EmailHistory_Report', 'Settings', 'Run email history reports'),
    ('EmailHistory_View',   'Settings', 'View email history'),

    ('EmailSetting_Create', 'Settings', 'Create email settings'),
    ('EmailSetting_Edit',   'Settings', 'Edit email settings'),
    ('EmailSetting_Delete', 'Settings', 'Delete email settings'),
    ('EmailSetting_Report', 'Settings', 'Run email setting reports'),
    ('EmailSetting_View',   'Settings', 'View email settings'),

    ('EmailTemplateParameter_Create', 'Settings', 'Create email template parameters'),
    ('EmailTemplateParameter_Edit',   'Settings', 'Edit email template parameters'),
    ('EmailTemplateParameter_Delete', 'Settings', 'Delete email template parameters'),
    ('EmailTemplateParameter_Report', 'Settings', 'Run email template parameter reports'),
    ('EmailTemplateParameter_View',   'Settings', 'View email template parameters'),

    ('EmailTemplate_Create', 'Settings', 'Create email templates'),
    ('EmailTemplate_Edit',   'Settings', 'Edit email templates'),
    ('EmailTemplate_Delete', 'Settings', 'Delete email templates'),
    ('EmailTemplate_Report', 'Settings', 'Run email template reports'),
    ('EmailTemplate_View',   'Settings', 'View email templates'),

    ('Incoterm_Create', 'Settings', 'Create incoterms'),
    ('Incoterm_Edit',   'Settings', 'Edit incoterms'),
    ('Incoterm_Delete', 'Settings', 'Delete incoterms'),
    ('Incoterm_Report', 'Settings', 'Run incoterm reports'),
    ('Incoterm_View',   'Settings', 'View incoterms'),

    ('ParamFormatOption_Create', 'Settings', 'Create param format options'),
    ('ParamFormatOption_Edit',   'Settings', 'Edit param format options'),
    ('ParamFormatOption_Delete', 'Settings', 'Delete param format options'),
    ('ParamFormatOption_Report', 'Settings', 'Run param format option reports'),
    ('ParamFormatOption_View',   'Settings', 'View param format options'),

    ('Port_Create', 'Settings', 'Create ports'),
    ('Port_Edit',   'Settings', 'Edit ports'),
    ('Port_Delete', 'Settings', 'Delete ports'),
    ('Port_Report', 'Settings', 'Run port reports'),
    ('Port_View',   'Settings', 'View ports'),

    ('SystemParameter_Create', 'Settings', 'Create system parameters'),
    ('SystemParameter_Edit',   'Settings', 'Edit system parameters'),
    ('SystemParameter_Delete', 'Settings', 'Delete system parameters'),
    ('SystemParameter_Report', 'Settings', 'Run system parameter reports'),
    ('SystemParameter_View',   'Settings', 'View system parameters'),

    ('UnitOfMeasurement_Create', 'Settings', 'Create units of measurement'),
    ('UnitOfMeasurement_Edit',   'Settings', 'Edit units of measurement'),
    ('UnitOfMeasurement_Delete', 'Settings', 'Delete units of measurement'),
    ('UnitOfMeasurement_Report', 'Settings', 'Run unit of measurement reports'),
    ('UnitOfMeasurement_View',   'Settings', 'View units of measurement'),

    ('Warehouse_Create', 'Settings', 'Create warehouses'),
    ('Warehouse_Edit',   'Settings', 'Edit warehouses'),
    ('Warehouse_Delete', 'Settings', 'Delete warehouses'),
    ('Warehouse_Report', 'Settings', 'Run warehouse reports'),
    ('Warehouse_View',   'Settings', 'View warehouses'),

    ('Module_Create', 'SystemAdministration', 'Create modules'),
    ('Module_Edit',   'SystemAdministration', 'Edit modules'),
    ('Module_Delete', 'SystemAdministration', 'Delete modules'),
    ('Module_Report', 'SystemAdministration', 'Run module reports'),

    ('Menu_Create', 'SystemAdministration', 'Create menus'),
    ('Menu_Edit',   'SystemAdministration', 'Edit menus'),
    ('Menu_Delete', 'SystemAdministration', 'Delete menus'),
    ('Menu_Report', 'SystemAdministration', 'Run menu reports');

INSERT INTO Identity_Permissions (Code, Module, ModuleId, Description)
SELECT p.Code, p.ModuleCode, m.Id, p.Description
FROM @Perms p
JOIN Setting_Modules m ON m.Code = p.ModuleCode
WHERE NOT EXISTS (SELECT 1 FROM Identity_Permissions WHERE Code = p.Code);

-- SuperAdmin + Admin: full access to every new permission.
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

-- Manager/Staff/Viewer: view-only on the new granular permissions.
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

-- Sidebar menu items pointed at the old catch-all Settings_View - repoint to the matching entity's new View code.
UPDATE Setting_Menus SET RequiredPermission = 'Currency_View' WHERE Controller = 'Currencies' AND RequiredPermission = 'Settings_View';
UPDATE Setting_Menus SET RequiredPermission = 'UnitOfMeasurement_View' WHERE Controller = 'UnitsOfMeasurement' AND RequiredPermission = 'Settings_View';
UPDATE Setting_Menus SET RequiredPermission = 'EmailSetting_View' WHERE Controller = 'EmailSettings' AND RequiredPermission = 'Settings_View';
UPDATE Setting_Menus SET RequiredPermission = 'EmailTemplate_View' WHERE Controller = 'EmailTemplates' AND RequiredPermission = 'Settings_View';
UPDATE Setting_Menus SET RequiredPermission = 'EmailHistory_View' WHERE Controller = 'EmailHistory' AND RequiredPermission = 'Settings_View';
GO
