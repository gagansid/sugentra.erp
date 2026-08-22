namespace Sugentra.ERP.UI.Models.Settings;

public record Module
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? ImageUrl { get; set; }
    public string? Route { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Color { get; set; }
    public decimal ColorOpacity { get; set; } = 1;
    public string? BackgroundColor { get; set; }
    public decimal BackgroundOpacity { get; set; } = 1;
    public string? TextColor { get; set; }
}
