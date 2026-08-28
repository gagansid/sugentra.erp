using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

// National/company holiday calendar — excluded (along with weekends) from business-day calculations
// such as the Approvals module's "Aging" column.
[Table("Setting_Holidays")]
public class Holiday : BaseAuditableEntity
{
    public DateTime Date { get; set; }
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}
