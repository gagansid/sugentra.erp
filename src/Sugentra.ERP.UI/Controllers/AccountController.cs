using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.UI.Auth;
using Sugentra.ERP.UI.Models.Account;
using Sugentra.ERP.UI.Services.Identity;

namespace Sugentra.ERP.UI.Controllers;

public class AccountController(AuthApiService authApiService, Sugentra.ERP.UI.Services.Settings.CompanyProfileApiService companyProfileApiService) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        await LoadBrandingAsync();
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    private async Task LoadBrandingAsync()
    {
        var branding = await companyProfileApiService.GetBrandingAsync();
        ViewBag.CompanyName = branding.Success ? branding.Data?.CompanyName : null;
        ViewBag.LogoUrl = branding.Success ? branding.Data?.LogoUrl : null;
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadBrandingAsync();
            return View(model);
        }

        var result = await authApiService.LoginAsync(model.Username, model.Password);
        if (!result.Success || result.Data is null)
        {
            // Message always comes straight from the API's ApiResponse envelope.
            ModelState.AddModelError(string.Empty, result.Message ?? "Login failed.");
            await LoadBrandingAsync();
            return View(model);
        }

        var token = result.Data;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value),
            new(ClaimTypes.Name, jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value),
            new(AuthClaimTypes.AccessToken, token.AccessToken),
            new(AuthClaimTypes.AccessTokenExpiresAt, token.AccessTokenExpiresAt.ToString("O")),
            new(AuthClaimTypes.RefreshToken, token.RefreshToken)
        };
        claims.AddRange(jwt.Claims.Where(c => c.Type == "permission")
            .Select(c => new Claim(AuthClaimTypes.Permission, c.Value)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = token.RefreshTokenExpiresAt
        });

        return !string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)
            ? Redirect(model.ReturnUrl)
            : RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = User.FindFirst(AuthClaimTypes.RefreshToken)?.Value;
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await authApiService.LogoutAsync(refreshToken);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword()
    {
        await LoadBrandingAsync();
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadBrandingAsync();
            return View(model);
        }

        var resetUrlBase = Url.Action(nameof(VerifyResetCode), "Account", null, Request.Scheme)!;
        var result = await authApiService.ForgotPasswordAsync(model.Email, resetUrlBase);

        TempData["SuccessMessage"] = result.Message ?? "If the email exists, a reset code has been sent.";
        return RedirectToAction(nameof(VerifyResetCode), new { email = model.Email });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyResetCode(string? email = null)
    {
        await LoadBrandingAsync();
        return View(new VerifyResetCodeViewModel { Email = email ?? string.Empty });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyResetCode(VerifyResetCodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadBrandingAsync();
            return View(model);
        }

        var result = await authApiService.ValidateResetCodeAsync(model.Email, model.Code);
        if (!result.Success)
        {
            await LoadBrandingAsync();
            ModelState.AddModelError(string.Empty, result.Message ?? "Invalid or expired reset code.");
            return View(model);
        }

        return RedirectToAction(nameof(ResetPassword), new { email = model.Email, code = model.Code });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendResetCode(string email)
    {
        var resetUrlBase = Url.Action(nameof(VerifyResetCode), "Account", null, Request.Scheme)!;
        var result = await authApiService.ForgotPasswordAsync(email, resetUrlBase);

        TempData["SuccessMessage"] = result.Message ?? "A new reset code has been sent.";
        return RedirectToAction(nameof(VerifyResetCode), new { email });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string? email = null, string? code = null)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
        {
            return RedirectToAction(nameof(ForgotPassword));
        }

        await LoadBrandingAsync();
        return View(new ResetPasswordViewModel { Email = email, Code = code });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadBrandingAsync();
            return View(model);
        }

        var result = await authApiService.ResetPasswordAsync(model.Email, model.Code, model.NewPassword, model.ConfirmPassword);
        if (!result.Success)
        {
            await LoadBrandingAsync();
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to reset password.");
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message ?? "Password reset successfully. Please sign in.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
