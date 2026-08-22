-- Drop the flat contact/address columns now that data has been migrated to the child tables (0010).
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'Address')
BEGIN
    ALTER TABLE MasterData_BusinessPartners DROP COLUMN Address;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'City')
BEGIN
    ALTER TABLE MasterData_BusinessPartners DROP COLUMN City;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'Country')
BEGIN
    ALTER TABLE MasterData_BusinessPartners DROP COLUMN Country;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE MasterData_BusinessPartners DROP COLUMN PhoneNumber;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'Email')
BEGIN
    ALTER TABLE MasterData_BusinessPartners DROP COLUMN Email;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MasterData_BusinessPartners') AND name = 'ContactPerson')
BEGIN
    ALTER TABLE MasterData_BusinessPartners DROP COLUMN ContactPerson;
END
