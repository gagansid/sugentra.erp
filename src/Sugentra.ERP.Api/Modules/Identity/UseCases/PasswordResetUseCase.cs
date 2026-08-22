using System.Security.Cryptography;
using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Logging;

namespace Sugentra.ERP.Api.Modules.Identity.UseCases;

public class PasswordResetUseCase(
    IUserRepository userRepository,
    IPasswordResetTokenRepository tokenRepository,
    IUserRefreshTokenRepository refreshTokenRepository,
    IEmailService emailService,
    ISystemParameterService systemParameterService,
    IPasswordHasher passwordHasher,
    IAuditLogService auditLogService)
{
    private const int CodeLength = 6;
    private const string TemplateCode = "PASSWORD_RESET";
    private const string ParameterCategory = "PasswordReset";
    private const int DefaultExpiryMinutes = 15;
    private static readonly char[] CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray(); // no 0/O/1/I ambiguity

    public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await userRepository.GetByUsernameOrEmailAsync(request.Email);

        // Never reveal whether the email exists - always return success to the caller.
        if (user is null || !user.IsActive)
        {
            return Result<bool>.Success(true);
        }

        var expiryMinutes = await systemParameterService.GetIntAsync(ParameterCategory, "ExpiresMinutes", DefaultExpiryMinutes);
        var code = GenerateCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        await tokenRepository.AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = expiresAt
        });

        var resetLink = $"{request.ResetUrlBase}?email={Uri.EscapeDataString(user.Email)}&code={code}";

        await emailService.SendAsync(new SendEmailRequest(
            ToEmail: user.Email,
            TemplateCode: TemplateCode,
            Placeholders: new Dictionary<string, object?>
            {
                ["FullName"] = user.FullName,
                ["Code"] = code,
                ["ResetLink"] = resetLink,
                ["ExpiresMinutes"] = expiryMinutes
            },
            SourceModule: "Identity",
            SourceReferenceId: user.Id));

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ValidateResetCodeAsync(ValidateResetCodeRequest request)
    {
        var (user, token) = await ResolveActiveTokenAsync(request.Email, request.Code);
        return user is null || token is null
            ? Result<bool>.Failure("Invalid or expired code.")
            : Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            return Result<bool>.Failure("Passwords do not match.");
        }

        var (user, token) = await ResolveActiveTokenAsync(request.Email, request.Code);
        if (user is null || token is null)
        {
            return Result<bool>.Failure("Invalid or expired code.");
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        await userRepository.UpdateAsync(user);
        await tokenRepository.MarkUsedAsync(token.Id);

        // Force re-login everywhere - a leaked/reset password shouldn't leave old sessions valid.
        foreach (var refreshToken in await refreshTokenRepository.GetByUserIdAsync(user.Id))
        {
            if (refreshToken.RevokedAt is null)
            {
                await refreshTokenRepository.RevokeAsync(refreshToken.Id);
            }
        }

        await auditLogService.LogAsync("Identity_Users", user.Id, "PasswordReset", null, null, null);

        return Result<bool>.Success(true);
    }

    private async Task<(User? User, PasswordResetToken? Token)> ResolveActiveTokenAsync(string email, string code)
    {
        var user = await userRepository.GetByUsernameOrEmailAsync(email);
        if (user is null)
        {
            return (null, null);
        }

        var token = await tokenRepository.GetActiveByUserAndCodeAsync(user.Id, code.ToUpperInvariant());
        return token is null ? (null, null) : (user, token);
    }

    private static string GenerateCode() =>
        new(Enumerable.Range(0, CodeLength).Select(_ => CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)]).ToArray());
}
