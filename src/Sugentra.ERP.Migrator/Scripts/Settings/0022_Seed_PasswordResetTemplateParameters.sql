-- Seed the ExpiresMinutes parameter for the PASSWORD_RESET template
-- (Modules/Identity/UseCases/PasswordResetUseCase.cs reads this via IEmailTemplateParameterService).
IF NOT EXISTS (
    SELECT 1
    FROM Setting_EmailTemplateParameters p
    INNER JOIN Setting_EmailTemplates t ON t.Id = p.EmailTemplateId
    WHERE t.Code = 'PASSWORD_RESET' AND p.ParamKey = 'ExpiresMinutes'
)
BEGIN
    INSERT INTO Setting_EmailTemplateParameters (EmailTemplateId, ParamKey, ParamValue, Description)
    SELECT Id, 'ExpiresMinutes', '15', 'How many minutes a password reset code stays valid.'
    FROM Setting_EmailTemplates
    WHERE Code = 'PASSWORD_RESET';
END
