-- Placeholder columns for future email/phone verification flow; no verification logic exists yet.
ALTER TABLE Identity_Users ADD EmailVerifiedAt DATETIME2 NULL;
ALTER TABLE Identity_Users ADD PhoneVerifiedAt DATETIME2 NULL;
