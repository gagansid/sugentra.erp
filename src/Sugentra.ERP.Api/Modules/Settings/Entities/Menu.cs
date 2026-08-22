using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_Menus")]
public class Menu : BaseAuditableEntity
{
    public long ModuleId { get; set; }
    public long? ParentId { get; set; }
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [StringLength(50)]
    public string? Icon { get; set; }
    [StringLength(100)]
    public string? Controller { get; set; }
    [StringLength(100)]
    public string? Action { get; set; }
    [StringLength(100)]
    public string? RequiredPermission { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
