namespace Sugentra.ERP.UI.Models.Settings;

public record EmailTemplateParameter
{
    public long Id { get; set; }
    public long EmailTemplateId { get; set; }
    public string ParamKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = "String";
    public bool IsLoop { get; set; }
    public string? FormatString { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
}
