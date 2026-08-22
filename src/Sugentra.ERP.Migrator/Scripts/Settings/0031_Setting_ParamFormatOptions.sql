-- Dedicated DataType -> allowed FormatString catalog (see Modules/Settings/Entities/ParamFormatOption.cs).
-- No CRUD UI is provided for this table by design - manage rows directly in the database.
IF OBJECT_ID('dbo.Setting_ParamFormatOptions') IS NULL
BEGIN
    CREATE TABLE Setting_ParamFormatOptions
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        DataType NVARCHAR(20) NOT NULL,
        FormatString NVARCHAR(50) NOT NULL,
        Label NVARCHAR(200) NULL,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy BIGINT NULL,
        UpdatedAt DATETIME2 NULL,
        UpdatedBy BIGINT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy BIGINT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Setting_ParamFormatOptions_DataType_FormatString')
BEGIN
    CREATE UNIQUE INDEX IX_Setting_ParamFormatOptions_DataType_FormatString
        ON Setting_ParamFormatOptions (DataType, FormatString)
        WHERE IsDeleted = 0;
END
