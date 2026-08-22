using System.ComponentModel.DataAnnotations;

namespace Sugentra.ERP.Api.Modules.Identity.Dtos;

public record ForgotPasswordRequest(
    [Required, StringLength(256)]
    string Email,

    // Base URL of the UI's reset-password page (e.g. "https://app.example.com/Account/ResetPassword") -
    // the API appends ?email=&code= to it so the emailed link works without the API knowing the UI's host.
    [Required]
    string ResetUrlBase);

public record ValidateResetCodeRequest(
    [Required, StringLength(256)]
    string Email,

    [Required, StringLength(6, MinimumLength = 6)]
    string Code);

public record ResetPasswordRequest(
    [Required, StringLength(256)]
    string Email,

    [Required, StringLength(6, MinimumLength = 6)]
    string Code,

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    string NewPassword,

    [Required]
    string ConfirmPassword);
