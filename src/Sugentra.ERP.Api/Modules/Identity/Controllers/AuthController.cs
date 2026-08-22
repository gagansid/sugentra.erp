using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.UseCases;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Identity.Controllers;

[Route("api/identity/auth")]
public class AuthController(AuthUseCase authUseCase, PasswordResetUseCase passwordResetUseCase) : ApiControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authUseCase.LoginAsync(request, GetDeviceInfo(), GetIpAddress());
        return result.IsSuccess
            ? Success(result.Value)
            : Failure(result.Error!, StatusCodes.Status401Unauthorized);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await authUseCase.RefreshAsync(request, GetDeviceInfo(), GetIpAddress());
        return result.IsSuccess
            ? Success(result.Value)
            : Failure(result.Error!, StatusCodes.Status401Unauthorized);
    }

    private string? GetDeviceInfo()
    {
        // The login/refresh call is made server-to-server from the UI's HttpClient, so Request.Headers.UserAgent
        // here reflects the UI server, not the actual browser. The UI forwards the real browser User-Agent via
        // this custom header (see AuthApiService) - prefer it, falling back to the raw header for direct API callers.
        var forwardedUserAgent = Request.Headers["X-Client-User-Agent"].ToString();
        var userAgent = !string.IsNullOrWhiteSpace(forwardedUserAgent) ? forwardedUserAgent : Request.Headers.UserAgent.ToString();
        return string.IsNullOrWhiteSpace(userAgent) ? null : userAgent;
    }

    private string? GetIpAddress()
    {
        // Trust X-Forwarded-For only if set (e.g. behind a reverse proxy); otherwise fall back to the direct connection.
        var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
        return !string.IsNullOrWhiteSpace(forwardedFor)
            ? forwardedFor.Split(',')[0].Trim()
            : HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var result = await authUseCase.LogoutAsync(request);
        return result.IsSuccess
            ? NoContent()
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        // Always returns success regardless of whether the email exists, to avoid user enumeration.
        await passwordResetUseCase.ForgotPasswordAsync(request);
        return SuccessMessage("If the email exists, a reset code has been sent.");
    }

    [HttpPost("validate-reset-code")]
    public async Task<IActionResult> ValidateResetCode([FromBody] ValidateResetCodeRequest request)
    {
        var result = await passwordResetUseCase.ValidateResetCodeAsync(request);
        return result.IsSuccess ? Success(true) : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await passwordResetUseCase.ResetPasswordAsync(request);
        return result.IsSuccess
            ? Success(true, "Password reset successfully. Please sign in with your new password.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }
}
