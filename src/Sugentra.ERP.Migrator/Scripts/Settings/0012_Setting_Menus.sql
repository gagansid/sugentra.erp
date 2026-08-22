-- Setting_Menus: menu items attached to a module, self-referencing ParentId for sub-menus.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_Menus')
BEGIN
    CREATE TABLE Setting_Menus
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_Menus PRIMARY KEY,
        ModuleId            BIGINT         NOT NULL CONSTRAINT FK_Setting_Menus_Module REFERENCES Setting_Modules(Id),
        ParentId            BIGINT         NULL CONSTRAINT FK_Setting_Menus_Parent REFERENCES Setting_Menus(Id),
        Name                NVARCHAR(100)  NOT NULL,
        Icon                NVARCHAR(50)   NULL,
        Controller          NVARCHAR(100)  NULL,
        Action              NVARCHAR(100)  NULL,
        RequiredPermission  NVARCHAR(100)  NULL,
        SortOrder           INT            NOT NULL CONSTRAINT DF_Setting_Menus_SortOrder DEFAULT (0),
        IsActive            BIT            NOT NULL CONSTRAINT DF_Setting_Menus_IsActive DEFAULT (1),

        CreatedAt           DATETIME2      NOT NULL CONSTRAINT DF_Setting_Menus_CreatedAt DEFAULT (GETDATE()),
        CreatedBy           BIGINT         NULL,
        UpdatedAt           DATETIME2      NULL,
        UpdatedBy           BIGINT         NULL,
        IsDeleted           BIT            NOT NULL CONSTRAINT DF_Setting_Menus_IsDeleted DEFAULT (0),
        DeletedAt           DATETIME2      NULL,
        DeletedBy           BIGINT         NULL
    );

    CREATE INDEX IX_Setting_Menus_ModuleId ON Setting_Menus (ModuleId);
    CREATE INDEX IX_Setting_Menus_ParentId ON Setting_Menus (ParentId);
END
GO
