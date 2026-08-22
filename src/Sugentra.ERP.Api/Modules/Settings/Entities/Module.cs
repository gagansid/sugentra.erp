using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_Modules")]
public class Module : BaseAuditableEntity
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [StringLength(50)]
    public string? Icon { get; set; }
    [StringLength(255)]
    public string? ImageUrl { get; set; }
    [StringLength(100)]
    public string? Route { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    [StringLength(20)]
    public string? Color { get; set; }
    [Range(0, 1)]
    public decimal ColorOpacity { get; set; } = 1;
    [StringLength(20)]
    public string? BackgroundColor { get; set; }
    [Range(0, 1)]
    public decimal BackgroundOpacity { get; set; } = 1;
    [StringLength(20)]
    public string? TextColor { get; set; }
}
