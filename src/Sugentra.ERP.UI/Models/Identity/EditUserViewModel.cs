using System.ComponentModel.DataAnnotations;

namespace Sugentra.ERP.UI.Models.Identity;

/// <summary>The API has no GET-by-id for Users, so Edit only carries the fields already known from the
/// list row (Email/FullName/IsActive). PhoneNumber/EmployeeId aren't in the list DTO and are left blank —
/// see the warning shown in the Edit view.</summary>
public class EditUserViewModel
{
    public long Id { get; set; }

    [Required, StringLength(100, MinimumLength = 3), RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username may only contain letters, digits, dot, underscore and hyphen.")]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmployeeId { get; set; }
}
