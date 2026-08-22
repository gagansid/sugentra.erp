-- The Procedures/ copy of usp_Setting_EmailTemplateParameters_GetByTemplateCode was already journaled
-- with the old ParamValue-based SELECT before ParamValue was dropped; re-issue it here (new filename,
-- so DbUp actually runs it) reading DefaultValue instead.
CREATE OR ALTER PROCEDURE usp_Setting_EmailTemplateParameters_GetByTemplateCode
    @TemplateCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.ParamKey, p.DefaultValue
    FROM Setting_EmailTemplateParameters p
    INNER JOIN Setting_EmailTemplates t ON t.Id = p.EmailTemplateId
    WHERE t.Code = @TemplateCode
      AND p.IsDeleted = 0
      AND t.IsDeleted = 0;
END
