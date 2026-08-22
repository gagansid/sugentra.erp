using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_Warehouses")]
public class Warehouse : BaseAuditableEntity
{
    [Required, StringLength(20)]
    public string Code { get; set; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Address { get; set; }
    [StringLength(100)]
    public string? City { get; set; }
    [StringLength(100)]
    public string? Country { get; set; }
    [Required, StringLength(20)]
    public string WarehouseType { get; set; } = "Main"; // Main | Quarantine | Vendor
    public bool IsActive { get; set; } = true;
}
