-- System-only module groupings so their menu items can attach to Setting_Menus. Each has its own workspace
-- landing page (consistent with Identity/Settings/MasterData), which also flips MenuQuery's HasWorkspace
-- flag to true so their sidebar groups become module-scoped (with "Back to Modules").
INSERT INTO Setting_Modules (Code, Name, Icon, Route, SortOrder, IsActive)
SELECT v.Code, v.Name, v.Icon, v.Route, v.SortOrder, 1
FROM (VALUES
    ('SystemAdministration', 'System Administration', 'ri-tools-line',       '/Workspace/SystemAdministration', 200),
    ('AuditLog',             'Audit Log',             'ri-history-line',     '/Workspace/AuditLog',             210),
    ('UIKit',                'UI Kit',                'ri-layout-grid-line', '/Workspace/UIKit',                220)
) AS v(Code, Name, Icon, Route, SortOrder)
WHERE NOT EXISTS (SELECT 1 FROM Setting_Modules WHERE Code = v.Code);
GO
