-- MasterData_BusinessPartnerContacts: one-to-many contacts (PIC) per partner.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MasterData_BusinessPartnerContacts')
BEGIN
    CREATE TABLE MasterData_BusinessPartnerContacts
    (
        Id                BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MasterData_BusinessPartnerContacts PRIMARY KEY,
        BusinessPartnerId BIGINT         NOT NULL,
        ContactType       NVARCHAR(20)   NOT NULL,
        ContactName       NVARCHAR(200)  NULL,
        Value             NVARCHAR(200)  NOT NULL,
        IsPrimary         BIT            NOT NULL CONSTRAINT DF_MasterData_BusinessPartnerContacts_IsPrimary DEFAULT (0),

        CreatedAt         DATETIME2      NOT NULL CONSTRAINT DF_MasterData_BusinessPartnerContacts_CreatedAt DEFAULT (GETDATE()),
        CreatedBy         BIGINT         NULL,
        UpdatedAt         DATETIME2      NULL,
        UpdatedBy         BIGINT         NULL,
        IsDeleted         BIT            NOT NULL CONSTRAINT DF_MasterData_BusinessPartnerContacts_IsDeleted DEFAULT (0),
        DeletedAt         DATETIME2      NULL,
        DeletedBy         BIGINT         NULL,

        CONSTRAINT FK_MasterData_BusinessPartnerContacts_Partner FOREIGN KEY (BusinessPartnerId) REFERENCES MasterData_BusinessPartners (Id),
        CONSTRAINT CK_MasterData_BusinessPartnerContacts_ContactType CHECK (ContactType IN ('Phone', 'WhatsApp', 'Fax', 'Email'))
    );
END
