-- MasterData_BillOfMaterials: BOM header, single-level (see 0004 for component lines). ItemId = the finished product this BOM produces.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MasterData_BillOfMaterials')
BEGIN
    CREATE TABLE MasterData_BillOfMaterials
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MasterData_BillOfMaterials PRIMARY KEY,
        ItemId        BIGINT         NOT NULL,
        Name          NVARCHAR(200)  NOT NULL,
        Description   NVARCHAR(500)  NULL,
        IsActive      BIT            NOT NULL CONSTRAINT DF_MasterData_BillOfMaterials_IsActive DEFAULT (1),

        CreatedAt     DATETIME2      NOT NULL CONSTRAINT DF_MasterData_BillOfMaterials_CreatedAt DEFAULT (GETDATE()),
        CreatedBy     BIGINT         NULL,
        UpdatedAt     DATETIME2      NULL,
        UpdatedBy     BIGINT         NULL,
        IsDeleted     BIT            NOT NULL CONSTRAINT DF_MasterData_BillOfMaterials_IsDeleted DEFAULT (0),
        DeletedAt     DATETIME2      NULL,
        DeletedBy     BIGINT         NULL,

        CONSTRAINT FK_MasterData_BillOfMaterials_Item FOREIGN KEY (ItemId) REFERENCES MasterData_Items (Id)
    );
END
