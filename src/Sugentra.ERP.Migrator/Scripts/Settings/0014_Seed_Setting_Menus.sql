-- Seed Setting_Menus with every item currently hardcoded in SidebarMenuDefinition.cs, so the sidebar can be
-- rendered fully from the DB. Idempotent: skipped per-row if a menu with the same Module+Controller+Action
-- already exists.
DECLARE @Rows TABLE (ModuleCode NVARCHAR(50), Name NVARCHAR(100), Icon NVARCHAR(50), Ctrl NVARCHAR(100), Act NVARCHAR(100), Perm NVARCHAR(100), SortOrder INT);
INSERT INTO @Rows (ModuleCode, Name, Icon, Ctrl, Act, Perm, SortOrder) VALUES
    ('Identity', 'Users', 'ri-group-line', 'Users', 'Index', 'User_View', 10),
    ('Identity', 'Roles', 'ri-team-line', 'Roles', 'Index', 'Role_View', 20),
    ('Identity', 'Permissions', 'ri-key-2-line', 'Permissions', 'Index', 'Permission_View', 30),

    ('MasterData', 'Business Partners', 'ri-briefcase-4-line', 'BusinessPartners', 'Index', 'MasterData_View', 10),
    ('MasterData', 'Items', 'ri-box-3-line', 'Items', 'Index', 'MasterData_View', 20),
    ('MasterData', 'Bill of Materials', 'ri-flow-chart', 'BillOfMaterials', 'Index', 'MasterData_View', 30),
    ('MasterData', 'Price Lists', 'ri-price-tag-3-line', 'PriceLists', 'Index', 'MasterData_View', 40),

    ('Settings', 'Currencies', 'ri-money-dollar-circle-line', 'Currencies', 'Index', 'Settings_View', 10),
    ('Settings', 'Units of Measurement', 'ri-ruler-2-line', 'UnitsOfMeasurement', 'Index', 'Settings_View', 20),

    ('SystemAdministration', 'Modules', 'ri-apps-2-line', 'Modules', 'Index', 'Module_View', 10),
    ('SystemAdministration', 'Menus', 'ri-menu-line', 'Menus', 'Index', 'Menu_View', 20),

    ('AuditLog', 'Audit Log', 'ri-file-list-3-line', 'AuditLogs', 'Index', 'AuditLog_View', 10),

    ('UIKit', 'Tables', 'ri-table-line', 'Tables', 'Basic', NULL, 10);

INSERT INTO Setting_Menus (ModuleId, ParentId, Name, Icon, Controller, Action, RequiredPermission, SortOrder)
SELECT mo.Id, NULL, r.Name, r.Icon, r.Ctrl, r.Act, r.Perm, r.SortOrder
FROM @Rows r
JOIN Setting_Modules mo ON mo.Code = r.ModuleCode
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_Menus me
    WHERE me.ModuleId = mo.Id AND me.Controller = r.Ctrl AND me.Action = r.Act
);
GO
