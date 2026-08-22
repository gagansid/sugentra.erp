using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Identity.Entities;

[Table("Identity_UserRoles")]
public class UserRole : BaseAuditableEntity
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
}
