-- Adds a free-text Remarks field (filled in by whoever has ErrorLog_Edit) and collapses Status down to just
-- two conditions: NotSolved (default) / Solved. EmailSent is no longer a condition - sending a notification
-- email is now a separate action (see Setting_EmailTemplates 'ERROR_LOG_NOTIFICATION') tracked via Setting_EmailHistory.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shared_ErrorLogs') AND name = 'Remarks')
BEGIN
    ALTER TABLE Shared_ErrorLogs ADD Remarks NVARCHAR(1000) NULL;
END
GO

UPDATE Shared_ErrorLogs SET Status = 'Solved' WHERE Status = 'Resolved';
UPDATE Shared_ErrorLogs SET Status = 'NotSolved' WHERE Status IN ('New', 'EmailSent');
GO

IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Shared_ErrorLogs') AND c.name = 'Status'
)
BEGIN
    DECLARE @ConstraintName NVARCHAR(200) = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Shared_ErrorLogs') AND c.name = 'Status'
    );
    EXEC('ALTER TABLE Shared_ErrorLogs DROP CONSTRAINT ' + @ConstraintName);
END
GO

ALTER TABLE Shared_ErrorLogs ADD CONSTRAINT DF_Shared_ErrorLogs_Status DEFAULT 'NotSolved' FOR Status;
GO
