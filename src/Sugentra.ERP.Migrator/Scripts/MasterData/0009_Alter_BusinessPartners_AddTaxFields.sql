-- Add Indonesian tax-identity fields to the core BusinessPartners table (1:1 attributes, not repeatable).
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'TaxRegisteredName')
BEGIN
    ALTER TABLE MasterData_BusinessPartners ADD TaxRegisteredName NVARCHAR(200) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'TaxAddress')
BEGIN
    ALTER TABLE MasterData_BusinessPartners ADD TaxAddress NVARCHAR(500) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'Nik')
BEGIN
    ALTER TABLE MasterData_BusinessPartners ADD Nik NVARCHAR(20) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'TaxpayerType')
BEGIN
    ALTER TABLE MasterData_BusinessPartners ADD TaxpayerType NVARCHAR(20) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'Nitku')
BEGIN
    ALTER TABLE MasterData_BusinessPartners ADD Nitku NVARCHAR(30) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'IsPkp')
BEGIN
    ALTER TABLE MasterData_BusinessPartners ADD IsPkp BIT NOT NULL CONSTRAINT DF_MasterData_BusinessPartners_IsPkp DEFAULT (0);
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'SktNumber')
BEGIN
    ALTER TABLE MasterData_BusinessPartners ADD SktNumber NVARCHAR(50) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'KluCode')
BEGIN
    ALTER TABLE MasterData_BusinessPartners ADD KluCode NVARCHAR(20) NULL;
END

GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_MasterData_BusinessPartners_TaxpayerType')
BEGIN
    ALTER TABLE MasterData_BusinessPartners ADD CONSTRAINT CK_MasterData_BusinessPartners_TaxpayerType
        CHECK (TaxpayerType IS NULL OR TaxpayerType IN ('Badan', 'OrangPribadi'));
END
