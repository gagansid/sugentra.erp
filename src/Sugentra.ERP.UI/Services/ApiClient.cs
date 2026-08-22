using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Sugentra.ERP.UI.Auth;
using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Identity;

namespace Sugentra.ERP.UI.Services;

/// <summary>Thin wrapper around HttpClient that talks to Sugentra.ERP.Api, attaches the Bearer token from the
/// current signed-in user's cookie claims, and unwraps the ApiResponse&lt;T&gt; envelope so Message always
/// comes straight from the API.</summary>
public class ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
{
    // API serializes with the default ASP.NET Core (camelCase) policy; case-insensitive matching lets our
    // PascalCase C# model properties deserialize without needing a custom naming policy on both sides.
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private async Task AttachBearerTokenAsync()
    {
        await EnsureFreshAccessTokenAsync();

        var token = httpContextAccessor.HttpContext?.User.FindFirst(AuthClaimTypes.AccessToken)?.Value;
        httpClient.DefaultRequestHeaders.Authorization = token is null
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    // The access token is short-lived (e.g. 5h) while the UI's auth cookie/refresh token lasts much longer (14d),
    // so a user who leaves the browser open/closed-and-reopened would otherwise start getting 401s from the API
    // well before the cookie itself expires. Silently refresh the access token here, ahead of any API call, using
    // the refresh token stored in the cookie, and re-issue the cookie with the new tokens.
    private async Task EnsureFreshAccessTokenAsync()
    {
        var context = httpContextAccessor.HttpContext;
        var user = context?.User;
        if (context is null || user?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var expiresAtClaim = user.FindFirst(AuthClaimTypes.AccessTokenExpiresAt)?.Value;
        if (!DateTime.TryParse(expiresAtClaim, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiresAt)
            || expiresAt > DateTime.UtcNow.AddMinutes(1))
        {
            return; // still valid (with a small buffer)
        }

        var refreshToken = user.FindFirst(AuthClaimTypes.RefreshToken)?.Value;
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        using var response = await httpClient.PostAsJsonAsync("api/identity/auth/refresh", new { RefreshToken = refreshToken });
        var result = await ReadAsync<TokenResponse>(response);
        if (!result.Success || result.Data is null)
        {
            return; // refresh token itself expired/invalid - the subsequent API call will 401 as before
        }

        var token = result.Data;
        var identity = (ClaimsIdentity)user.Identity!;
        ReplaceClaim(identity, AuthClaimTypes.AccessToken, token.AccessToken);
        ReplaceClaim(identity, AuthClaimTypes.AccessTokenExpiresAt, token.AccessTokenExpiresAt.ToString("O"));
        ReplaceClaim(identity, AuthClaimTypes.RefreshToken, token.RefreshToken);

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = token.RefreshTokenExpiresAt
        });
    }

    private static void ReplaceClaim(ClaimsIdentity identity, string type, string value)
    {
        var existing = identity.FindFirst(type);
        if (existing is not null)
        {
            identity.RemoveClaim(existing);
        }

        identity.AddClaim(new Claim(type, value));
    }

    public async Task<ApiResult<T>> GetAsync<T>(string requestUri)
    {
        await AttachBearerTokenAsync();
        using var response = await httpClient.GetAsync(requestUri);
        return await ReadAsync<T>(response);
    }

    public async Task<ApiResult<T>> PostAsync<T>(string requestUri, object? body = null)
    {
        await AttachBearerTokenAsync();
        using var response = await httpClient.PostAsJsonAsync(requestUri, body);
        return await ReadAsync<T>(response);
    }

    // Overload used by AuthApiService.LoginAsync to forward the real browser's User-Agent/IP as headers -
    // this call happens before authentication, so it deliberately skips AttachBearerTokenAsync/token refresh.
    public async Task<ApiResult<T>> PostAsync<T>(string requestUri, object? body, IDictionary<string, string> extraHeaders)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = JsonContent.Create(body) };
        foreach (var (key, value) in extraHeaders)
        {
            request.Headers.TryAddWithoutValidation(key, value);
        }

        using var response = await httpClient.SendAsync(request);
        return await ReadAsync<T>(response);
    }

    public async Task<ApiResult<T>> PutAsync<T>(string requestUri, object? body = null)
    {
        await AttachBearerTokenAsync();
        using var response = await httpClient.PutAsJsonAsync(requestUri, body);
        return await ReadAsync<T>(response);
    }

    public async Task<ApiResult<bool>> DeleteAsync(string requestUri)
    {
        await AttachBearerTokenAsync();
        using var response = await httpClient.DeleteAsync(requestUri);
        return await ReadAsync<bool>(response);
    }

    public async Task<ApiResult<T>> PostFileAsync<T>(string requestUri, IFormFile file, IDictionary<string, string>? fields = null)
    {
        await AttachBearerTokenAsync();
        using var content = new MultipartFormDataContent();
        using var fileStream = file.OpenReadStream();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);

        if (fields is not null)
        {
            foreach (var (key, value) in fields)
            {
                content.Add(new StringContent(value), key);
            }
        }

        using var response = await httpClient.PostAsync(requestUri, content);
        return await ReadAsync<T>(response);
    }

    private static async Task<ApiResult<T>> ReadAsync<T>(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;

        // 204 No Content (e.g. logout) has no body to deserialize.
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return new ApiResult<T> { Success = response.IsSuccessStatusCode, StatusCode = statusCode };
        }

        ApiResponse<T>? envelope;
        try
        {
            envelope = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        }
        catch (System.Text.Json.JsonException)
        {
            envelope = null;
        }

        return new ApiResult<T>
        {
            Success = envelope?.Success ?? response.IsSuccessStatusCode,
            Message = envelope?.Message,
            Data = envelope is null ? default : envelope.Data,
            Errors = envelope?.Errors,
            StatusCode = statusCode
        };
    }
}
