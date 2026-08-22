using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

// Generic app-wide config store (distinct from EmailTemplateParameter, which is a per-template placeholder
// catalog, not a runtime value). ParamCategory groups related keys (e.g. "PasswordReset"); null = uncategorized.
[Table("Setting_SystemParameters")]
public class SystemParameter : BaseAuditableEntity
{
    [StringLength(50)]
    public string? ParamCategory { get; set; }
    [Required, StringLength(100)]
    public string ParamKey { get; set; } = string.Empty;
    [StringLength(500)]
    public string? ParamValue { get; set; }
    [StringLength(500)]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
