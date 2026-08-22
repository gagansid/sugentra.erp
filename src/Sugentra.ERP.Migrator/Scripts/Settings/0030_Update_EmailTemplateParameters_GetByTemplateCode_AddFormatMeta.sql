-- The Procedures/ copy was already journaled with the old (ParamKey, DefaultValue) SELECT before DataType/FormatString
-- were added to the projection; re-issue it here (new filename, so DbUp actually runs it) so EmailService.Render can
-- apply Shared.Common.ParamValueFormatter using each placeholder's DataType/FormatString.
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
