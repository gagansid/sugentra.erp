using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_Currencies")]
public class Currency : BaseAuditableEntity
{
    [Required, StringLength(3, MinimumLength = 3)]
    public string Code { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [StringLength(10)]
    public string? Symbol { get; set; }
    public bool IsActive { get; set; } = true;
}
