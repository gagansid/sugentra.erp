-- Generic app-wide config lookup (distinct from Setting_EmailTemplateParameters, which is a per-template
-- placeholder catalog, not a runtime value store). ParamCategory NULL matches uncategorized parameters.
CREATE OR ALTER PROCEDURE usp_Setting_SystemParameters_GetByKey
    @ParamCategory NVARCHAR(50) = NULL,
    @ParamKey NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 ParamValue
    FROM Setting_SystemParameters
    WHERE ParamKey = @ParamKey
      AND ISNULL(ParamCategory, N'') = ISNULL(@ParamCategory, N'')
      AND IsDeleted = 0
      AND IsActive = 1;
END
