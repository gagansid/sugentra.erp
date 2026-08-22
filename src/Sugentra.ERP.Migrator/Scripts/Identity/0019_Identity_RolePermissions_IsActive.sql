-- Identity_RolePermissions: add IsActive flag (assign/revoke toggles this instead of soft-deleting the grant row)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Identity_RolePermissions') AND name = 'IsActive')
BEGIN
    ALTER TABLE Identity_RolePermissions ADD IsActive BIT NOT NULL CONSTRAINT DF_Identity_RolePermissions_IsActive DEFAULT (1);
END
