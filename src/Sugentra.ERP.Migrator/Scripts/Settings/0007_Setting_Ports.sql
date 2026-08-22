-- Setting_Ports: loading/discharge ports used by future Export Documentation / Logistics modules.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_Ports')
BEGIN
    CREATE TABLE Setting_Ports
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_Ports PRIMARY KEY,
        Code          NVARCHAR(10)   NOT NULL,
        Name          NVARCHAR(200)  NOT NULL,
        Country       NVARCHAR(100)  NULL,
        IsActive      BIT            NOT NULL CONSTRAINT DF_Setting_Ports_IsActive DEFAULT (1),

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Setting_Ports_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Setting_Ports_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT UQ_Setting_Ports_Code UNIQUE (Code)
    );
END
