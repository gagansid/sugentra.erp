-- Seed: sample reference + master data so Currencies/Units/Business Partners/Items/Price Lists/BOMs
-- aren't empty out of the box. Self-contained in one script (rather than split across the
-- Settings/MasterData folders) so DbUp's alphabetical script ordering can't run the MasterData
-- inserts before the Settings ones they depend on (Currencies/UnitsOfMeasurement must exist first).
-- Partner names are generic aliases, not real/legal company names.

INSERT INTO Setting_Currencies (Code, Name, Symbol, CreatedBy)
SELECT v.Code, v.Name, v.Symbol, NULL
FROM (VALUES
    ('USD', 'US Dollar',         '$'),
    ('IDR', 'Indonesian Rupiah', 'Rp'),
    ('EUR', 'Euro',              '€'),
    ('SGD', 'Singapore Dollar',  'S$')
) AS v(Code, Name, Symbol)
WHERE NOT EXISTS (SELECT 1 FROM Setting_Currencies WHERE Code = v.Code);
GO

INSERT INTO Setting_UnitsOfMeasurement (Code, Name, CreatedBy)
SELECT v.Code, v.Name, NULL
FROM (VALUES
    ('KG',   'Kilogram'),
    ('MT',   'Metric Ton'),
    ('PCS',  'Pieces'),
    ('CBM',  'Cubic Meter'),
    ('SET',  'Set'),
    ('ROLL', 'Roll')
) AS v(Code, Name)
WHERE NOT EXISTS (SELECT 1 FROM Setting_UnitsOfMeasurement WHERE Code = v.Code);
GO

INSERT INTO MasterData_BusinessPartners (Code, Name, PartnerType, Address, City, Country, PhoneNumber, Email, ContactPerson, CreatedBy)
SELECT v.Code, v.Name, v.PartnerType, v.Address, v.City, v.Country, v.PhoneNumber, v.Email, v.ContactPerson, NULL
FROM (VALUES
    ('BP-0001', 'Rimba Rotan',           'Supplier', 'Jl. Perindustrian No. 12', 'Cirebon',      'Indonesia',  '+62 231 555 1010', 'sales@rimbarotan.example',    'Budi Santoso'),
    ('BP-0002', 'Anggun Kriya',          'Customer', 'Jl. Craft Center No. 5',   'Jakarta',      'Indonesia',  '+62 21 555 2020',  'order@anggunkriya.example',   'Siti Amalia'),
    ('BP-0003', 'Borneo Alam Raya',      'Supplier', 'Jl. Sungai Kahayan No. 8', 'Palangkaraya', 'Indonesia',  '+62 536 555 3030', 'supply@borneoalam.example',   'Yohanes Tarung'),
    ('BP-0004', 'Nusa Craft Export',     'Customer', 'Jl. Pemuda No. 21',        'Semarang',     'Indonesia',  '+62 24 555 4040',  'export@nusacraft.example',    'Dewi Lestari'),
    ('BP-0005', 'Global Rattan Trading', 'Customer', '21 Woodlands Ave',         'Singapore',    'Singapore',  '+65 6555 5050',    'buyer@globalrattan.example',  'Wei Ling Tan'),
    ('BP-0006', 'Java Handicraft',       'Both',     'Jl. Industri Raya No. 3',  'Surabaya',     'Indonesia',  '+62 31 555 6060',  'contact@javahandicraft.example', 'Agus Prasetyo')
) AS v(Code, Name, PartnerType, Address, City, Country, PhoneNumber, Email, ContactPerson)
WHERE NOT EXISTS (SELECT 1 FROM MasterData_BusinessPartners WHERE Code = v.Code);
GO

INSERT INTO MasterData_Items (Code, Name, Category, Grade, UnitOfMeasurementId, StandardPrice, CreatedBy)
SELECT v.Code, v.Name, v.Category, v.Grade, uom.Id, v.StandardPrice, NULL
FROM (VALUES
    ('ITM-0001', 'Raw Rattan Core - Grade A', 'RawMaterial',   'A',        'KG',  25000.00),
    ('ITM-0002', 'Rattan Peel Skin',          'RawMaterial',   'B',        'KG',  18000.00),
    ('ITM-0003', 'Woven Rattan Sheet',        'SemiFinished',  'Standard', 'PCS', 45000.00),
    ('ITM-0004', 'Rattan Chair - Classic',    'FinishedGood',  'Premium',  'PCS', 850000.00),
    ('ITM-0005', 'Rattan Basket Set',         'FinishedGood',  'Standard', 'SET', 320000.00),
    ('ITM-0006', 'Rattan Lamp Shade',         'FinishedGood',  'Standard', 'PCS', 275000.00)
) AS v(Code, Name, Category, Grade, UomCode, StandardPrice)
JOIN Setting_UnitsOfMeasurement uom ON uom.Code = v.UomCode
WHERE NOT EXISTS (SELECT 1 FROM MasterData_Items WHERE Code = v.Code);
GO

INSERT INTO MasterData_PriceLists (ItemId, CurrencyId, Price, EffectiveDate, CreatedBy)
SELECT item.Id, cur.Id, v.Price, CAST(GETDATE() AS DATE), NULL
FROM (VALUES
    ('ITM-0004', 'USD', 65.00),
    ('ITM-0004', 'IDR', 850000.00),
    ('ITM-0005', 'USD', 24.00),
    ('ITM-0005', 'IDR', 320000.00),
    ('ITM-0006', 'USD', 20.00)
) AS v(ItemCode, CurrencyCode, Price)
JOIN MasterData_Items item ON item.Code = v.ItemCode
JOIN Setting_Currencies cur ON cur.Code = v.CurrencyCode
WHERE NOT EXISTS (
    SELECT 1 FROM MasterData_PriceLists pl
    WHERE pl.ItemId = item.Id AND pl.CurrencyId = cur.Id AND pl.EffectiveDate = CAST(GETDATE() AS DATE)
);
GO

INSERT INTO MasterData_BillOfMaterials (ItemId, Name, Description, CreatedBy)
SELECT item.Id, v.Name, v.Description, NULL
FROM (VALUES
    ('ITM-0004', 'Rattan Chair - Classic BOM', 'Standard assembly BOM for the Classic rattan chair.'),
    ('ITM-0005', 'Rattan Basket Set BOM',      'Standard assembly BOM for the rattan basket set.')
) AS v(ItemCode, Name, Description)
JOIN MasterData_Items item ON item.Code = v.ItemCode
WHERE NOT EXISTS (SELECT 1 FROM MasterData_BillOfMaterials WHERE Name = v.Name);
GO

INSERT INTO MasterData_BillOfMaterialItems (BillOfMaterialId, ComponentItemId, Quantity, UnitOfMeasurementId, Notes, CreatedBy)
SELECT bom.Id, component.Id, v.Quantity, uom.Id, v.Notes, NULL
FROM (VALUES
    ('Rattan Chair - Classic BOM', 'ITM-0001', 3.0000, 'KG',  'Frame core material'),
    ('Rattan Chair - Classic BOM', 'ITM-0003', 2.0000, 'PCS', 'Woven seat/back panels'),
    ('Rattan Basket Set BOM',      'ITM-0002', 1.5000, 'KG',  'Basket weave material'),
    ('Rattan Basket Set BOM',      'ITM-0003', 1.0000, 'PCS', 'Reinforced base panel')
) AS v(BomName, ComponentCode, Quantity, UomCode, Notes)
JOIN MasterData_BillOfMaterials bom ON bom.Name = v.BomName
JOIN MasterData_Items component ON component.Code = v.ComponentCode
JOIN Setting_UnitsOfMeasurement uom ON uom.Code = v.UomCode
WHERE NOT EXISTS (
    SELECT 1 FROM MasterData_BillOfMaterialItems bi
    WHERE bi.BillOfMaterialId = bom.Id AND bi.ComponentItemId = component.Id
);
GO
