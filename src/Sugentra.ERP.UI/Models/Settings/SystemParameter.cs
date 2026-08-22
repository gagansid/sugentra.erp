namespace Sugentra.ERP.UI.Models.Settings;

public record SystemParameter
{
    public long Id { get; set; }
    public string? ParamCategory { get; set; }
    public string ParamKey { get; set; } = string.Empty;
    public string? ParamValue { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public record SystemParameterListItemDto(
    long Id, string? ParamCategory, string ParamKey, string? ParamValue, string? Description, bool IsActive);

public record SystemParameterListRequest(
    string? Category = null, string? Search = null, bool? IsActive = null, int Page = 1, int PageSize = 10);
