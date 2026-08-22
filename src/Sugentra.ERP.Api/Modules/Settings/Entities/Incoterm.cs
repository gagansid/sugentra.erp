using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_Incoterms")]
public class Incoterm : BaseAuditableEntity
{
    [Required, StringLength(10)]
    public string Code { get; set; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
