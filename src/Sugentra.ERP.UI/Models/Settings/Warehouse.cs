namespace Sugentra.ERP.UI.Models.Settings;

public record Warehouse
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string WarehouseType { get; set; } = "Main"; // Main | Quarantine | Vendor
    public bool IsActive { get; set; } = true;
}
