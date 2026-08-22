-- Tracks investigation state of an error log entry: New (default), Resolved, EmailSent.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shared_ErrorLogs') AND name = 'Status')
BEGIN
    ALTER TABLE Shared_ErrorLogs ADD Status NVARCHAR(20) NOT NULL DEFAULT 'New';
END
GO
