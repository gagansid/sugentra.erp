using Microsoft.Extensions.Options;
using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Repositories;
using Sugentra.ERP.Api.Modules.Identity.Services;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Logging;

namespace Sugentra.ERP.Api.Modules.Identity.UseCases;

public class AuthUseCase(
    IUserRepository userRepository,
    IUserRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    PermissionResolverService permissionResolverService,
    IAuditLogService auditLogService,
    IOptions<AccountLockoutOptions> lockoutOptions)
{
    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request, string? deviceInfo = null, string? ipAddress = null)
    {
        var user = await userRepository.GetByUsernameOrEmailAsync(request.Username);
        if (user is null || !user.IsActive)
        {
            return Result<TokenResponse>.Failure("Invalid username or password.");
        }

        if (user.LockoutEnd is not null)
        {
            return Result<TokenResponse>.Failure("Account is locked due to too many failed login attempts. Contact an administrator to unlock it.");
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await RegisterFailedAttemptAsync(user);
            return Result<TokenResponse>.Failure("Invalid username or password.");
        }

        if (user.FailedLoginAttempts > 0 || user.LockoutEnd is not null)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        return Result<TokenResponse>.Success(await IssueTokensAsync(user, deviceInfo, ipAddress));
    }

    public async Task<Result<TokenResponse>> RefreshAsync(RefreshTokenRequest request, string? deviceInfo = null, string? ipAddress = null)
    {
        var tokenHash = jwtTokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash);
        if (storedToken is null || storedToken.RevokedAt is not null || storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return Result<TokenResponse>.Failure("Invalid or expired refresh token.");
        }

        var user = await userRepository.GetByIdAsync(storedToken.UserId);
        if (user is null || !user.IsActive)
        {
            return Result<TokenResponse>.Failure("Invalid or expired refresh token.");
        }

        // Rotate: revoke the used refresh token, issue a brand new access+refresh pair.
        await refreshTokenRepository.RevokeAsync(storedToken.Id);

        return Result<TokenResponse>.Success(await IssueTokensAsync(user, deviceInfo, ipAddress));
    }

    public async Task<Result<bool>> LogoutAsync(RefreshTokenRequest request)
    {
        var tokenHash = jwtTokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash);
        if (storedToken is null)
        {
            return Result<bool>.Failure("Refresh token not found.");
        }

        await refreshTokenRepository.RevokeAsync(storedToken.Id);
        return Result<bool>.Success(true);
    }

    private async Task RegisterFailedAttemptAsync(User user)
    {
        user.FailedLoginAttempts++;

        if (user.FailedLoginAttempts >= lockoutOptions.Value.MaxFailedAttempts)
        {
            // Locked indefinitely; only an admin with User_Unlock can clear this (see UserUseCase.UnlockAsync).
            user.LockoutEnd = DateTime.MaxValue;
            user.FailedLoginAttempts = 0;
            await userRepository.UpdateAsync(user);
            await auditLogService.LogAsync("Identity_Users", user.Id, "AccountLocked", null, null, null);
            return;
        }

        await userRepository.UpdateAsync(user);
    }

    private async Task<TokenResponse> IssueTokensAsync(User user, string? deviceInfo = null, string? ipAddress = null)
    {
        var permissions = await permissionResolverService.ResolveAsync(user.Id);
        var (accessToken, accessTokenExpiresAt) = jwtTokenService.GenerateAccessToken(user.Id, user.Username, permissions);

        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.Add(jwtTokenService.RefreshTokenLifetime);

        await refreshTokenRepository.AddAsync(new UserRefreshToken
        {
            UserId = user.Id,
            TokenHash = jwtTokenService.HashRefreshToken(refreshToken),
            ExpiresAt = refreshTokenExpiresAt,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            CreatedBy = user.Id
        });

        return new TokenResponse(accessToken, accessTokenExpiresAt, refreshToken, refreshTokenExpiresAt);
    }
}
