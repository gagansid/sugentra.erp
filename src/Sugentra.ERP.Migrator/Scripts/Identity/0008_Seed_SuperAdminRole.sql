-- Seed: SuperAdmin role only. The Super Admin user itself (with BCrypt password hash)
-- is seeded from Migrator/Program.cs in C#, since password hashing needs BCrypt.Net-Next.
IF NOT EXISTS (SELECT 1 FROM Identity_Roles WHERE Name = 'SuperAdmin')
BEGIN
    INSERT INTO Identity_Roles (Name, Description, CreatedBy)
    VALUES ('SuperAdmin', 'Full system access, bypasses per-permission checks in Phase 1.', NULL);
END
