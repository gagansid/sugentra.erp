-- Enforce "at most one primary address per type" / "at most one primary contact per type" at the DB level
-- (previously JS-only). Filtered unique indexes only apply to rows where IsPrimary = 1 AND IsDeleted = 0, so
-- non-primary and soft-deleted rows are unaffected and multiple partners/types can each still have their own primary.

-- Dedupe first: if legacy/JS-only enforcement ever let two rows of the same (BusinessPartnerId, AddressType) both
-- be IsPrimary = 1, keep the lowest Id as primary and demote the rest so the unique index below can be created.
;WITH DupeAddresses AS (
    SELECT Id,
           ROW_NUMBER() OVER (PARTITION BY BusinessPartnerId, AddressType ORDER BY Id) AS rn
    FROM MasterData_BusinessPartnerAddresses
    WHERE IsPrimary = 1 AND IsDeleted = 0
)
UPDATE a
SET IsPrimary = 0
FROM MasterData_BusinessPartnerAddresses a
JOIN DupeAddresses d ON d.Id = a.Id
WHERE d.rn > 1;

;WITH DupeContacts AS (
    SELECT Id,
           ROW_NUMBER() OVER (PARTITION BY BusinessPartnerId, ContactType ORDER BY Id) AS rn
    FROM MasterData_BusinessPartnerContacts
    WHERE IsPrimary = 1 AND IsDeleted = 0
)
UPDATE c
SET IsPrimary = 0
FROM MasterData_BusinessPartnerContacts c
JOIN DupeContacts d ON d.Id = c.Id
WHERE d.rn > 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_MasterData_BusinessPartnerAddresses_OnePrimaryPerType')
BEGIN
    CREATE UNIQUE INDEX UQ_MasterData_BusinessPartnerAddresses_OnePrimaryPerType
        ON MasterData_BusinessPartnerAddresses (BusinessPartnerId, AddressType)
        WHERE IsPrimary = 1 AND IsDeleted = 0;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_MasterData_BusinessPartnerContacts_OnePrimaryPerType')
BEGIN
    CREATE UNIQUE INDEX UQ_MasterData_BusinessPartnerContacts_OnePrimaryPerType
        ON MasterData_BusinessPartnerContacts (BusinessPartnerId, ContactType)
        WHERE IsPrimary = 1 AND IsDeleted = 0;
END
