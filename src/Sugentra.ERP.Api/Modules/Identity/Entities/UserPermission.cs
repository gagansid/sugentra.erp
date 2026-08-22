using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Identity.Entities;

[Table("Identity_UserPermissions")]
public class UserPermission : BaseAuditableEntity
{
    public long UserId { get; set; }
    public long PermissionId { get; set; }
    // true = explicit allow-override, false = explicit deny-override (deny always wins — see PermissionResolverService).
    public bool IsAllowed { get; set; }
}
