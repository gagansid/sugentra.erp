namespace Sugentra.ERP.Api.Modules.Identity.Dtos;

public record LoginRequest(string Username, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record TokenResponse(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);
