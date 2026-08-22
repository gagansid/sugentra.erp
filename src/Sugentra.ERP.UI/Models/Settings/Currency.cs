namespace Sugentra.ERP.UI.Models.Settings;

public record Currency
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public bool IsActive { get; set; } = true;
}
