-- Setting_Warehouses.WarehouseType: distinguishes Main/Quarantine/Vendor locations for Inventory flows (see docs/modules/inventory.md open decisions).
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Setting_Warehouses') AND name = 'WarehouseType')
BEGIN
    ALTER TABLE Setting_Warehouses
        ADD WarehouseType NVARCHAR(20) NOT NULL CONSTRAINT DF_Setting_Warehouses_WarehouseType DEFAULT ('Main');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Setting_Warehouses_WarehouseType')
BEGIN
    ALTER TABLE Setting_Warehouses
        ADD CONSTRAINT CK_Setting_Warehouses_WarehouseType CHECK (WarehouseType IN ('Main', 'Quarantine', 'Vendor'));
END
