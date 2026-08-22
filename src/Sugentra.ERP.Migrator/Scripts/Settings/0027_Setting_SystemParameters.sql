-- IF-guarded since a prior failed run of this script may have already created the table before erroring.
IF OBJECT_ID('dbo.Setting_SystemParameters') IS NULL
BEGIN
    CREATE TABLE Setting_SystemParameters
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        ParamCategory NVARCHAR(50) NULL,
        ParamKey NVARCHAR(100) NOT NULL,
        ParamValue NVARCHAR(500) NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy BIGINT NULL,
        UpdatedAt DATETIME2 NULL,
        UpdatedBy BIGINT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy BIGINT NULL,
        -- SQL Server can't index an expression directly; persist ISNULL(ParamCategory, '') so NULL
        -- (uncategorized) parameters still participate in the uniqueness check as their own group.
        ParamCategoryKey AS (ISNULL(ParamCategory, N'')) PERSISTED
    );
END
GO

-- A prior failed run of this script may have created the table without the computed column.
IF COL_LENGTH('dbo.Setting_SystemParameters', 'ParamCategoryKey') IS NULL
BEGIN
    ALTER TABLE Setting_SystemParameters ADD ParamCategoryKey AS (ISNULL(ParamCategory, N'')) PERSISTED;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Setting_SystemParameters_Category_Key')
BEGIN
    CREATE UNIQUE INDEX IX_Setting_SystemParameters_Category_Key
        ON Setting_SystemParameters (ParamCategoryKey, ParamKey)
        WHERE IsDeleted = 0;
END

