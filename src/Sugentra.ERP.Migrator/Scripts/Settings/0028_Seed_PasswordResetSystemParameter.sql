-- ExpiresMinutes now lives here (runtime config), not in Setting_EmailTemplateParameters (placeholder catalog).
INSERT INTO Setting_SystemParameters (ParamCategory, ParamKey, ParamValue, Description)
SELECT 'PasswordReset', 'ExpiresMinutes', '15', 'How many minutes a password reset code stays valid.'
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_SystemParameters
    WHERE ParamCategory = 'PasswordReset' AND ParamKey = 'ExpiresMinutes'
);
