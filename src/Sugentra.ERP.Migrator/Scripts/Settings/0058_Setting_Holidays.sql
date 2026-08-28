-- Setting_Holidays: national/company holiday calendar, excluded (along with weekends) from
-- business-day calculations such as the Approvals module's "Aging" column.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_Holidays')
BEGIN
    CREATE TABLE Setting_Holidays
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_Holidays PRIMARY KEY,
        [Date]        DATE           NOT NULL,
        Name          NVARCHAR(200)  NOT NULL,

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Setting_Holidays_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Setting_Holidays_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT UQ_Setting_Holidays_Date UNIQUE ([Date])
    );
END
