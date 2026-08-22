using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_Ports")]
public class Port : BaseAuditableEntity
{
    [Required, StringLength(10)]
    public string Code { get; set; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [StringLength(100)]
    public string? Country { get; set; }
    public bool IsActive { get; set; } = true;
}
