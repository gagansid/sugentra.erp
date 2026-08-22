-- Placeholder catalog for ERROR_LOG_NOTIFICATION (ErrorLogsController's SendEmail action supplies all of these
-- dynamically per entry, so there is no DefaultValue).
INSERT INTO Setting_EmailTemplateParameters (EmailTemplateId, ParamKey, DataType, IsRequired, Description)
SELECT t.Id, v.ParamKey, v.DataType, 1, v.Description
FROM Setting_EmailTemplates t
CROSS JOIN (VALUES
    ('EntryId', 'Int', 'The error log entry''s Id.'),
    ('Source', 'String', 'Where the error was reported from (Api/UI).'),
    ('OccurredAt', 'Date', 'When the error occurred.'),
    ('Endpoint', 'String', 'The request endpoint that failed.'),
    ('ExceptionType', 'String', 'The .NET exception type name.'),
    ('Status', 'String', 'The entry''s condition - NotSolved or Solved.'),
    ('Remarks', 'String', 'Free-text remarks entered by an ErrorLog_Edit user.'),
    ('Message', 'String', 'The exception message.')
) AS v(ParamKey, DataType, Description)
WHERE t.Code = 'ERROR_LOG_NOTIFICATION'
  AND NOT EXISTS (
      SELECT 1 FROM Setting_EmailTemplateParameters p
      WHERE p.EmailTemplateId = t.Id AND p.ParamKey = v.ParamKey
  );
