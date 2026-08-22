-- Tracks who last triggered the "Send Email" notification action, shown on the Detail page.
ALTER TABLE Shared_ErrorLogs ADD EmailSentBy BIGINT NULL;
