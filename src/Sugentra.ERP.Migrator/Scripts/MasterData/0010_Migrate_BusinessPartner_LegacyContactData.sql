-- Move legacy flat Address/PhoneNumber/Email/ContactPerson values into the new child tables before dropping those columns.
INSERT INTO MasterData_BusinessPartnerAddresses (BusinessPartnerId, AddressType, Address, City, Country, IsPrimary, CreatedBy)
SELECT Id, 'Office', Address, City, Country, 1, CreatedBy
FROM MasterData_BusinessPartners bp
WHERE bp.Address IS NOT NULL AND bp.IsDeleted = 0
  AND NOT EXISTS (SELECT 1 FROM MasterData_BusinessPartnerAddresses a WHERE a.BusinessPartnerId = bp.Id);

INSERT INTO MasterData_BusinessPartnerContacts (BusinessPartnerId, ContactType, ContactName, Value, IsPrimary, CreatedBy)
SELECT Id, 'Phone', ContactPerson, PhoneNumber, 1, CreatedBy
FROM MasterData_BusinessPartners bp
WHERE bp.PhoneNumber IS NOT NULL AND bp.IsDeleted = 0
  AND NOT EXISTS (SELECT 1 FROM MasterData_BusinessPartnerContacts c WHERE c.BusinessPartnerId = bp.Id AND c.ContactType = 'Phone');

INSERT INTO MasterData_BusinessPartnerContacts (BusinessPartnerId, ContactType, ContactName, Value, IsPrimary, CreatedBy)
SELECT Id, 'Email', ContactPerson, Email, 1, CreatedBy
FROM MasterData_BusinessPartners bp
WHERE bp.Email IS NOT NULL AND bp.IsDeleted = 0
  AND NOT EXISTS (SELECT 1 FROM MasterData_BusinessPartnerContacts c WHERE c.BusinessPartnerId = bp.Id AND c.ContactType = 'Email');
