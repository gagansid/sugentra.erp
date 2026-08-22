-- Identity_Roles: add IsActive flag
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Identity_Roles') AND name = 'IsActive')
BEGIN
    ALTER TABLE Identity_Roles ADD IsActive BIT NOT NULL CONSTRAINT DF_Identity_Roles_IsActive DEFAULT (1);
END
