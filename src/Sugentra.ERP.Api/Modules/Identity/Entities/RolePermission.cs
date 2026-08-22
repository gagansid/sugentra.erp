using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Identity.Entities;

[Table("Identity_RolePermissions")]
public class RolePermission : BaseAuditableEntity
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public bool IsActive { get; set; } = true;
}
