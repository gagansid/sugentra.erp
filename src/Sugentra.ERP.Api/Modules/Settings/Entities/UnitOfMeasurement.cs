using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_UnitsOfMeasurement")]
public class UnitOfMeasurement : BaseAuditableEntity
{
    [Required, StringLength(20)]
    public string Code { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
