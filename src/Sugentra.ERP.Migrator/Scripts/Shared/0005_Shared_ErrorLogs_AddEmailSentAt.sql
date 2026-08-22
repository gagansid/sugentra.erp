-- Tracks when the "Send Email" notification action was last used for an entry, shown on the Detail page.
ALTER TABLE Shared_ErrorLogs ADD EmailSentAt DATETIME2 NULL;
