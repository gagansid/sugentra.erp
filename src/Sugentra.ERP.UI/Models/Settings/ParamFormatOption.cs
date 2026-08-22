namespace Sugentra.ERP.UI.Models.Settings;

public record ParamFormatOption
{
    public long Id { get; set; }
    public string DataType { get; set; } = string.Empty;
    public string FormatString { get; set; } = string.Empty;
    public string? Label { get; set; }
    public int SortOrder { get; set; }
}
