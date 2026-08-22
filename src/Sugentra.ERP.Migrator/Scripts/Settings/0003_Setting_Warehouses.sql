-- Setting_Warehouses: physical/logical stock locations referenced by future Inventory module.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Setting_Warehouses')
BEGIN
    CREATE TABLE Setting_Warehouses
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting_Warehouses PRIMARY KEY,
        Code          NVARCHAR(20)   NOT NULL,
        Name          NVARCHAR(200)  NOT NULL,
        Address       NVARCHAR(500)  NULL,
        City          NVARCHAR(100)  NULL,
        Country       NVARCHAR(100)  NULL,
        IsActive      BIT            NOT NULL CONSTRAINT DF_Setting_Warehouses_IsActive DEFAULT (1),

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_Setting_Warehouses_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_Setting_Warehouses_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT UQ_Setting_Warehouses_Code UNIQUE (Code)
    );
END
