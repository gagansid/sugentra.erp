-- Adds a proper FK (Identity_Permissions.ModuleId -> Setting_Modules.Id) alongside the existing free-text
-- Module column, so module/hub matching can rely on referential integrity instead of a string compare that
-- silently fails on typos/casing drift. Lives under Settings/ (not Identity/) because the Migrator always runs
-- every Identity script before any Settings script (see ScriptModuleOrderComparer in Sugentra.ERP.Migrator/Program.cs) -
-- Setting_Modules doesn't exist yet while Identity scripts run, so this FK can only be added from here.
-- Additive/backward-compatible: the existing Module string column is left untouched, nothing else changes.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Identity_Permissions') AND name = 'ModuleId')
BEGIN
    ALTER TABLE Identity_Permissions ADD ModuleId BIGINT NULL;
END
GO

-- Backfill existing rows by matching the current free-text Module value against Setting_Modules.Code.
UPDATE p
SET p.ModuleId = m.Id
FROM Identity_Permissions p
JOIN Setting_Modules m ON m.Code = p.Module
WHERE p.ModuleId IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Identity_Permissions_Module')
BEGIN
    ALTER TABLE Identity_Permissions
        ADD CONSTRAINT FK_Identity_Permissions_Module FOREIGN KEY (ModuleId) REFERENCES Setting_Modules (Id);
END
GO
