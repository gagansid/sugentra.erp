-- Allows recording a submission with no matching Approval Flow (auto-approved, no levels), so the
-- document's approval history still shows who submitted it even when no flow applies.
ALTER TABLE Approval_Requests DROP CONSTRAINT FK_Approval_Requests_FlowDefinition;
ALTER TABLE Approval_Requests ALTER COLUMN FlowDefinitionId BIGINT NULL;
ALTER TABLE Approval_Requests ADD CONSTRAINT FK_Approval_Requests_FlowDefinition
    FOREIGN KEY (FlowDefinitionId) REFERENCES Approval_FlowDefinitions (Id);
GO
