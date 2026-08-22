using Sugentra.ERP.UI.Models.Identity;

namespace Sugentra.ERP.UI.Services.Identity;

public class AuthApiService(ApiClient apiClient, IHttpContextAccessor httpContextAccessor)
{
    public Task<ApiResult<TokenResponse>> LoginAsync(string username, string password)
    {
        // Forward the real browser's User-Agent/IP as custom headers - this is a server-to-server call, so
        // without forwarding, the API would only see the UI server's own HttpClient request (see AuthController).
        var request = httpContextAccessor.HttpContext?.Request;
        var userAgent = request?.Headers.UserAgent.ToString();
        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        var extraHeaders = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(userAgent)) extraHeaders["X-Client-User-Agent"] = userAgent;
        if (!string.IsNullOrWhiteSpace(ipAddress)) extraHeaders["X-Forwarded-For"] = ipAddress;

        return apiClient.PostAsync<TokenResponse>("api/identity/auth/login", new LoginRequest(username, password), extraHeaders);
    }

    public Task<ApiResult<bool>> LogoutAsync(string refreshToken) =>
        apiClient.PostAsync<bool>("api/identity/auth/logout", new { RefreshToken = refreshToken });

    public Task<ApiResult<bool>> ForgotPasswordAsync(string email, string resetUrlBase) =>
        apiClient.PostAsync<bool>("api/identity/auth/forgot-password", new { Email = email, ResetUrlBase = resetUrlBase });

    public Task<ApiResult<bool>> ValidateResetCodeAsync(string email, string code) =>
        apiClient.PostAsync<bool>("api/identity/auth/validate-reset-code", new { Email = email, Code = code });

    public Task<ApiResult<bool>> ResetPasswordAsync(string email, string code, string newPassword, string confirmPassword) =>
        apiClient.PostAsync<bool>("api/identity/auth/reset-password", new { Email = email, Code = code, NewPassword = newPassword, ConfirmPassword = confirmPassword });
}
