namespace Sugentra.ERP.Api.Modules.Settings.Queries;

public static class EmailTemplateParameterQuery
{
    public const string GetByTemplateCodeProcedureName = "usp_Setting_EmailTemplateParameters_GetByTemplateCode";
}

public record EmailTemplateParameterRow(string ParamKey, string? DefaultValue, string DataType, string? FormatString);
