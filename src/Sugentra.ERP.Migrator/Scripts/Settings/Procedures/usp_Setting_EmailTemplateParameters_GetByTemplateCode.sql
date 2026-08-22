-- usp_Setting_EmailTemplateParameters_GetByTemplateCode: cross-module lookup used by Identity's
-- PasswordResetUseCase (via IEmailTemplateParameterService) to read per-template runtime settings
-- (e.g. ExpiresMinutes) without depending on Modules/Settings/** directly.
CREATE OR ALTER PROCEDURE usp_Setting_EmailTemplateParameters_GetByTemplateCode
    @TemplateCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.ParamKey, p.DefaultValue, p.DataType, p.FormatString
    FROM Setting_EmailTemplateParameters p
    INNER JOIN Setting_EmailTemplates t ON t.Id = p.EmailTemplateId
    WHERE t.Code = @TemplateCode
      AND p.IsDeleted = 0
      AND t.IsDeleted = 0;
END

