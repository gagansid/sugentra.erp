namespace Sugentra.ERP.UI.Models.Settings;

public record Menu
{
    public long Id { get; set; }
    public long ModuleId { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Controller { get; set; }
    public string? Action { get; set; }
    public string? RequiredPermission { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
