using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;

namespace Sugentra.ERP.UI.Services.Settings;

public class CurrencyApiService(ApiClient apiClient) : CrudApiService<Currency>(apiClient, "api/settings/currencies");

public class HolidayApiService(ApiClient apiClient) : CrudApiService<Holiday>(apiClient, "api/settings/holidays");

public class UnitOfMeasurementApiService(ApiClient apiClient) : CrudApiService<UnitOfMeasurement>(apiClient, "api/settings/units-of-measurement");

public class WarehouseApiService(ApiClient apiClient) : CrudApiService<Warehouse>(apiClient, "api/settings/warehouses");

public class DocumentNumberingApiService(ApiClient apiClient) : CrudApiService<DocumentNumbering>(apiClient, "api/settings/document-numberings");

public class EmailSettingApiService(ApiClient apiClient) : CrudApiService<EmailSetting>(apiClient, "api/settings/email-settings")
{
    // ApiResult<object?> so a null "data" on failure responses (Test never returns real data) deserializes cleanly.
    public Task<ApiResult<object?>> TestAsync(EmailSetting settings, string toEmail, string? subject = null, string? body = null) =>
        ApiClient.PostAsync<object?>("api/settings/email-settings/test", new { Settings = settings, ToEmail = toEmail, Subject = subject, Body = body });
}

public class EmailTemplateApiService(ApiClient apiClient) : CrudApiService<EmailTemplate>(apiClient, "api/settings/email-templates");

public class EmailTemplateParameterApiService(ApiClient apiClient) : CrudApiService<EmailTemplateParameter>(apiClient, "api/settings/email-template-parameters")
{
    public Task<ApiResult<IReadOnlyList<EmailTemplateParameter>>> GetByTemplateIdAsync(long emailTemplateId) =>
        ApiClient.GetAsync<IReadOnlyList<EmailTemplateParameter>>($"api/settings/email-template-parameters?emailTemplateId={emailTemplateId}");
}

public class SystemParameterApiService(ApiClient apiClient) : CrudApiService<SystemParameter>(apiClient, "api/settings/system-parameters")
{
    public Task<ApiResult<IReadOnlyList<SystemParameter>>> GetByCategoryAsync(string category) =>
        ApiClient.GetAsync<IReadOnlyList<SystemParameter>>($"api/settings/system-parameters?category={Uri.EscapeDataString(category)}");

    public Task<ApiResult<PagedResult<SystemParameterListItemDto>>> GetPagedAsync(SystemParameterListRequest request)
    {
        var query = $"api/settings/system-parameters/paged?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.Category) ? "" : $"&Category={Uri.EscapeDataString(request.Category)}")
            + (string.IsNullOrWhiteSpace(request.Search) ? "" : $"&Search={Uri.EscapeDataString(request.Search)}")
            + (request.IsActive is null ? "" : $"&IsActive={request.IsActive}");

        return ApiClient.GetAsync<PagedResult<SystemParameterListItemDto>>(query);
    }
}

// Read-only (no CRUD UI) - rows are seeded/managed directly in the database.
public class ParamFormatOptionApiService(ApiClient apiClient)
{
    public Task<ApiResult<IReadOnlyList<ParamFormatOption>>> GetByDataTypeAsync(string dataType) =>
        apiClient.GetAsync<IReadOnlyList<ParamFormatOption>>($"api/settings/param-format-options?dataType={Uri.EscapeDataString(dataType)}");
}


// Singleton config, not a list - exposes just the branding-scoped GetIdentityAsync used by the dashboard.
public class CompanyProfileApiService(ApiClient apiClient, IConfiguration configuration)
{
    public async Task<ApiResult<CompanyIdentity>> GetIdentityAsync() =>
        AbsolutizeLogo(await apiClient.GetAsync<CompanyIdentity>("api/settings/company-profile/identity"));

    // Same payload, reachable without a bearer token - used by the pre-login sign-in page.
    public async Task<ApiResult<CompanyIdentity>> GetBrandingAsync() =>
        AbsolutizeLogo(await apiClient.GetAsync<CompanyIdentity>("api/settings/company-profile/branding"));

    public Task<ApiResult<CompanyProfile>> GetAsync() =>
        apiClient.GetAsync<CompanyProfile>("api/settings/company-profile");

    public Task<ApiResult<CompanyProfile>> UpdateAsync(CompanyProfile request) =>
        apiClient.PutAsync<CompanyProfile>("api/settings/company-profile", request);

    // LogoUrl comes back as a relative "/uploads/..." path from the API - the UI runs on a different
    // origin, so it must be prefixed with the API base URL before it can be used in an <img src>.
    private ApiResult<CompanyIdentity> AbsolutizeLogo(ApiResult<CompanyIdentity> result)
    {
        if (result.Success && result.Data is { LogoUrl: { } logoUrl } data && logoUrl.StartsWith('/'))
        {
            var baseUrl = configuration["Api:BaseUrl"]?.TrimEnd('/');
            return new ApiResult<CompanyIdentity>
            {
                Success = result.Success,
                Message = result.Message,
                Data = data with { LogoUrl = $"{baseUrl}{logoUrl}" },
                Errors = result.Errors,
                StatusCode = result.StatusCode
            };
        }

        return result;
    }
}


