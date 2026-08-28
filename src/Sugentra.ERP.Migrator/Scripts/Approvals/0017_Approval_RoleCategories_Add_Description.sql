-- Adds an optional human-readable label per Approver Type (DocumentType), stored on every row sharing that
-- DocumentType since Approval_RoleCategories has no separate DocumentType catalog table.
ALTER TABLE Approval_RoleCategories ADD Description NVARCHAR(200) NULL;
GO
