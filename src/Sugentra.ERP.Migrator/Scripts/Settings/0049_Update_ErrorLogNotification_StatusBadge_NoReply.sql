-- Adds a colored status badge (StatusBadge, HTML snippet) to the Condition row and a "do not reply" footer note.
UPDATE Setting_EmailTemplates
SET BodyHtml = '<table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f6f8;padding:40px 0;font-family:Arial,Helvetica,sans-serif;"><tr><td align="center"><table role="presentation" width="560" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #e5e7eb;"><tr><td align="center" style="background-color:#d9480f;padding:24px;"><span style="color:#ffffff;font-size:18px;font-weight:bold;">Error Log Notification</span></td></tr><tr><td style="padding:24px;color:#1f2937;font-size:14px;line-height:1.6;"><p>Entry <strong>#{{EntryId}}</strong> ({{Source}}) requires attention.</p><table role="presentation" width="100%" cellpadding="6" cellspacing="0" style="font-size:13px;border-collapse:collapse;"><tr><td style="width:140px;color:#6b7280;">Occurred At</td><td>{{OccurredAt}}</td></tr><tr><td style="color:#6b7280;">Endpoint</td><td>{{Endpoint}}</td></tr><tr><td style="color:#6b7280;">Exception Type</td><td>{{ExceptionType}}</td></tr><tr><td style="color:#6b7280;">Condition</td><td>{{StatusBadge}}</td></tr><tr><td style="color:#6b7280;">Remarks</td><td>{{Remarks}}</td></tr></table><p style="margin-top:16px;">Message:</p><pre style="white-space:pre-wrap;background-color:#f4f6f8;padding:12px;border-radius:4px;font-size:12px;">{{Message}}</pre><p style="margin-top:16px;color:#9ca3af;font-size:11px;">This is an automated message — please do not reply to this email.</p></td></tr></table></td></tr></table>',
    BodyText = 'Entry #{{EntryId}} ({{Source}}) requires attention. Occurred At: {{OccurredAt}}. Endpoint: {{Endpoint}}. Exception Type: {{ExceptionType}}. Condition: {{Status}}. Remarks: {{Remarks}}. Message: {{Message}} This is an automated message - please do not reply to this email.',
    AvailablePlaceholders = 'EntryId,Source,OccurredAt,Endpoint,ExceptionType,Status,StatusBadge,Remarks,Message'
WHERE Code = 'ERROR_LOG_NOTIFICATION';

INSERT INTO Setting_EmailTemplateParameters (EmailTemplateId, ParamKey, DataType, IsRequired, Description)
SELECT t.Id, 'StatusBadge', 'String', 1, 'Pre-rendered HTML badge for the entry''s condition (Not Solved/Solved).'
FROM Setting_EmailTemplates t
WHERE t.Code = 'ERROR_LOG_NOTIFICATION'
  AND NOT EXISTS (
      SELECT 1 FROM Setting_EmailTemplateParameters p
      WHERE p.EmailTemplateId = t.Id AND p.ParamKey = 'StatusBadge'
  );
