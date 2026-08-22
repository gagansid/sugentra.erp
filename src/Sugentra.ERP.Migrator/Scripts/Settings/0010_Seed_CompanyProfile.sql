-- Seed: default single Setting_CompanyProfile row (Id=1) so the entity is never queried against an empty table.
IF NOT EXISTS (SELECT 1 FROM Setting_CompanyProfile)
BEGIN
    INSERT INTO Setting_CompanyProfile (CompanyName, Country, LogoUrl, CreatedBy)
    VALUES ('Sugentra ERP', 'Indonesia', '/uploads/branding/logo.png', NULL);
END
