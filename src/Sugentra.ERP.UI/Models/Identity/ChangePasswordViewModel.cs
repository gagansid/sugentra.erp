using System.ComponentModel.DataAnnotations;

namespace Sugentra.ERP.UI.Models.Identity;

public class ChangePasswordViewModel
{
    public long Id { get; set; }

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
