-- Setting_Modules: dynamic registry of ERP modules, drives the post-login module hub grid + sidebar top-level grouping.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_Modules')
BEGIN
    CREATE TABLE Setting_Modules
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_Modules PRIMARY KEY,
        Code                NVARCHAR(50)   NOT NULL,
        Name                NVARCHAR(100)  NOT NULL,
        Icon                NVARCHAR(50)   NULL,
        ImageUrl            NVARCHAR(255)  NULL,
        Route               NVARCHAR(100)  NULL,
        SortOrder           INT            NOT NULL CONSTRAINT DF_Setting_Modules_SortOrder DEFAULT (0),
        IsActive            BIT            NOT NULL CONSTRAINT DF_Setting_Modules_IsActive DEFAULT (1),

        -- Per-module hub tile color customization (icon color, box background, box/icon opacity, name text color).
        Color               NVARCHAR(20)   NULL,
        ColorOpacity        DECIMAL(3,2)   NOT NULL CONSTRAINT DF_Setting_Modules_ColorOpacity DEFAULT (1.00),
        BackgroundColor     NVARCHAR(20)   NULL,
        BackgroundOpacity   DECIMAL(3,2)   NOT NULL CONSTRAINT DF_Setting_Modules_BackgroundOpacity DEFAULT (1.00),
        TextColor           NVARCHAR(20)   NULL,

        CreatedAt           DATETIME2      NOT NULL CONSTRAINT DF_Setting_Modules_CreatedAt DEFAULT (GETDATE()),
        CreatedBy           BIGINT         NULL,
        UpdatedAt           DATETIME2      NULL,
        UpdatedBy           BIGINT         NULL,
        IsDeleted           BIT            NOT NULL CONSTRAINT DF_Setting_Modules_IsDeleted DEFAULT (0),
        DeletedAt           DATETIME2      NULL,
        DeletedBy           BIGINT         NULL,

        CONSTRAINT UQ_Setting_Modules_Code UNIQUE (Code)
    );
END
GO

-- Seed: module catalog matching the folders under Modules/ (see docs/plan.md). Idempotent re-run safe.
-- Icon = Remix Icon (ri-*) name from the bundled iconify set. Route only set for modules with a working workspace.
INSERT INTO Setting_Modules (Code, Name, Icon, Route, SortOrder)
SELECT v.Code, v.Name, v.Icon, v.Route, v.SortOrder
FROM (VALUES
    ('Identity',        'Identity & Access',  'ri-shield-user-line',        '/Workspace/Identity',    10),
    ('Settings',        'Settings',            'ri-settings-3-line',         '/Workspace/Settings',    20),
    ('MasterData',      'Master Data',         'ri-database-2-line',         '/Workspace/MasterData',  30),
    ('Procurement',     'Procurement',         'ri-shopping-cart-2-line',    NULL,                      40),
    ('Inventory',       'Inventory',           'ri-archive-2-line',          NULL,                      50),
    ('Production',      'Production',          'ri-tools-line',              NULL,                      60),
    ('Sales',           'Sales',               'ri-line-chart-line',         NULL,                      70),
    ('ExportDocuments', 'Export Documents',    'ri-file-text-line',          NULL,                      80),
    ('Logistics',       'Logistics',           'ri-truck-line',              NULL,                      90),
    ('QualityControl',  'Quality Control',     'ri-checkbox-circle-line',    NULL,                     100),
    ('Finance',         'Finance',             'ri-money-dollar-circle-line',NULL,                     110),
    ('HR',              'HR',                  'ri-team-line',               NULL,                     120),
    ('Reporting',       'Reporting',           'ri-bar-chart-2-line',        NULL,                     130)
) AS v(Code, Name, Icon, Route, SortOrder)
WHERE NOT EXISTS (SELECT 1 FROM Setting_Modules WHERE Code = v.Code);
GO

