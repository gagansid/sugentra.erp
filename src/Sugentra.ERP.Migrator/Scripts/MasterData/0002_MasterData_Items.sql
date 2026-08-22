-- MasterData_Items: rotan mentah / setengah jadi / barang jadi, referenced by BOM/PriceList/future Inventory.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MasterData_Items')
BEGIN
    CREATE TABLE MasterData_Items
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MasterData_Items PRIMARY KEY,
        Code                 NVARCHAR(30)   NOT NULL,
        Name                 NVARCHAR(200)  NOT NULL,
        Category             NVARCHAR(30)   NOT NULL,
        Grade                NVARCHAR(50)   NULL,
        UnitOfMeasurementId  BIGINT         NOT NULL,
        StandardPrice        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_MasterData_Items_StandardPrice DEFAULT (0),
        IsActive             BIT            NOT NULL CONSTRAINT DF_MasterData_Items_IsActive DEFAULT (1),

        CreatedAt            DATETIME2      NOT NULL CONSTRAINT DF_MasterData_Items_CreatedAt DEFAULT (GETDATE()),
        CreatedBy            BIGINT         NULL,
        UpdatedAt            DATETIME2      NULL,
        UpdatedBy            BIGINT         NULL,
        IsDeleted            BIT            NOT NULL CONSTRAINT DF_MasterData_Items_IsDeleted DEFAULT (0),
        DeletedAt            DATETIME2      NULL,
        DeletedBy            BIGINT         NULL,

        CONSTRAINT UQ_MasterData_Items_Code UNIQUE (Code),
        CONSTRAINT CK_MasterData_Items_Category CHECK (Category IN ('RawMaterial', 'SemiFinished', 'FinishedGood')),
        CONSTRAINT FK_MasterData_Items_UnitOfMeasurement FOREIGN KEY (UnitOfMeasurementId) REFERENCES Setting_UnitsOfMeasurement (Id)
    );
END
