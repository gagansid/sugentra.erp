-- Action was NVARCHAR(20), too narrow for longer action names (e.g. "ApprovalLevelAdvanced",
-- "RemovePermissionOverride"). Widen with headroom rather than raising it every time a new name is added.
ALTER TABLE Setting_AuditLogs ALTER COLUMN Action NVARCHAR(50) NOT NULL;
GO
