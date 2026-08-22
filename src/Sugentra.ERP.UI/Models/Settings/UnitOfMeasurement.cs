namespace Sugentra.ERP.UI.Models.Settings;

public record UnitOfMeasurement
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
