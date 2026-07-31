using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Identity.Entities;

[Table("Identity_Users")]
public class User : BaseAuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
