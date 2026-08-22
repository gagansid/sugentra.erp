using System.ComponentModel.DataAnnotations;

namespace Sugentra.ERP.UI.Models.Identity;

public class CreateUserViewModel
{
    [Required, StringLength(100, MinimumLength = 3), RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username may only contain letters, digits, dot, underscore and hyphen.")]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public string? EmployeeId { get; set; }
}
