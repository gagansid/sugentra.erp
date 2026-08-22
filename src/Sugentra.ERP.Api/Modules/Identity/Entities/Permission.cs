using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Identity.Entities;

[Table("Identity_Permissions")]
public class Permission : BaseAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string? Description { get; set; }

    // FK to Setting_Modules.Id - added alongside the existing free-text Module column (kept as-is for
    // backward compatibility) so module matching can rely on referential integrity instead of a string compare.
    public long? ModuleId { get; set; }
}
