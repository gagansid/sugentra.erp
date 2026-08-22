-- MasterData_BillOfMaterialItems: BOM component lines (single-level — no nested sub-BOMs yet).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MasterData_BillOfMaterialItems')
BEGIN
    CREATE TABLE MasterData_BillOfMaterialItems
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MasterData_BillOfMaterialItems PRIMARY KEY,
        BillOfMaterialId     BIGINT         NOT NULL,
        ComponentItemId      BIGINT         NOT NULL,
        Quantity             DECIMAL(18,4)  NOT NULL,
        UnitOfMeasurementId  BIGINT         NOT NULL,
        Notes                NVARCHAR(500)  NULL,

        CreatedAt            DATETIME2      NOT NULL CONSTRAINT DF_MasterData_BillOfMaterialItems_CreatedAt DEFAULT (GETDATE()),
        CreatedBy            BIGINT         NULL,
        UpdatedAt            DATETIME2      NULL,
        UpdatedBy            BIGINT         NULL,
        IsDeleted            BIT            NOT NULL CONSTRAINT DF_MasterData_BillOfMaterialItems_IsDeleted DEFAULT (0),
        DeletedAt            DATETIME2      NULL,
        DeletedBy            BIGINT         NULL,

        CONSTRAINT FK_MasterData_BillOfMaterialItems_Bom FOREIGN KEY (BillOfMaterialId) REFERENCES MasterData_BillOfMaterials (Id),
        CONSTRAINT FK_MasterData_BillOfMaterialItems_ComponentItem FOREIGN KEY (ComponentItemId) REFERENCES MasterData_Items (Id),
        CONSTRAINT FK_MasterData_BillOfMaterialItems_UnitOfMeasurement FOREIGN KEY (UnitOfMeasurementId) REFERENCES Setting_UnitsOfMeasurement (Id)
    );
END
