-- Sets the ExpiresMinutes default (lost when ParamValue was dropped) and seeds the remaining
-- PASSWORD_RESET placeholders as a catalog (Modules/Identity/UseCases/PasswordResetUseCase.cs sends
-- FullName/Code/ResetLink dynamically per user; ExpiresMinutes always uses this DefaultValue).
UPDATE p
SET p.DataType = 'Int', p.DefaultValue = '15', p.Description = 'How many minutes a password reset code stays valid.'
FROM Setting_EmailTemplateParameters p
INNER JOIN Setting_EmailTemplates t ON t.Id = p.EmailTemplateId
WHERE t.Code = 'PASSWORD_RESET' AND p.ParamKey = 'ExpiresMinutes';

INSERT INTO Setting_EmailTemplateParameters (EmailTemplateId, ParamKey, DataType, IsRequired, Description)
SELECT t.Id, v.ParamKey, v.DataType, 1, v.Description
FROM Setting_EmailTemplates t
CROSS JOIN (VALUES
    ('FullName', 'String', 'The user''s full name.'),
    ('Code', 'String', 'The 6-character password reset code.'),
    ('ResetLink', 'String', 'Direct link to the reset-password page, pre-filled with email and code.')
) AS v(ParamKey, DataType, Description)
WHERE t.Code = 'PASSWORD_RESET'
  AND NOT EXISTS (
      SELECT 1 FROM Setting_EmailTemplateParameters p
      WHERE p.EmailTemplateId = t.Id AND p.ParamKey = v.ParamKey
  );
