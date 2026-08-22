-- Setting_UnitsOfMeasurement: e.g. KG, MT, PCS, CBM — used by future Master Data (Items) module.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_UnitsOfMeasurement')
BEGIN
    CREATE TABLE Setting_UnitsOfMeasurement
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_UnitsOfMeasurement PRIMARY KEY,
        Code          NVARCHAR(20)   NOT NULL,
        Name          NVARCHAR(100)  NOT NULL,
        IsActive      BIT            NOT NULL CONSTRAINT DF_Setting_UnitsOfMeasurement_IsActive DEFAULT (1),

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Setting_UnitsOfMeasurement_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Setting_UnitsOfMeasurement_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT UQ_Setting_UnitsOfMeasurement_Code UNIQUE (Code)
    );
END
