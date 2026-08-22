-- 0036 tagged the Email* permissions with ModuleCode 'Settings' by mistake - their Setting_Menus entries
-- (0019) actually live under the 'SystemAdministration' module, so the dashboard module-hub tile and the
-- Permissions/Roles module filter must agree with that, not with the generic Settings grouping.
UPDATE p
SET p.Module = 'SystemAdministration', p.ModuleId = m.Id
FROM Identity_Permissions p
JOIN Setting_Modules m ON m.Code = 'SystemAdministration'
WHERE p.Code LIKE 'EmailSetting[_]%'
   OR p.Code LIKE 'EmailTemplate[_]%'
   OR p.Code LIKE 'EmailTemplateParameter[_]%'
   OR p.Code LIKE 'EmailHistory[_]%';
GO
