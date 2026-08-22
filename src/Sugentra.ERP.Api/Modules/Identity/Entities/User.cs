using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Identity.Entities;

[Table("Identity_Users")]
public class User : BaseAuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // Never serialized into audit log JSON snapshots or API responses.
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? EmployeeId { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; } = true;
    // Verification is not implemented yet; these columns exist so the UI can display a verified badge once it is.
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? PhoneVerifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    // Permanent admin action, distinct from IsActive (a plain administrative disable) and LockoutEnd (temporary auto-lock).
    public bool IsBanned { get; set; }
}
