using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;

namespace Sugentra.ERP.UI.Services.Settings;

public class EmailHistoryApiService(ApiClient apiClient)
{
    public Task<ApiResult<PagedResult<EmailHistoryListItemDto>>> GetPagedAsync(EmailHistoryListRequest request)
    {
        var query = $"api/settings/email-history?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.ToEmail) ? "" : $"&ToEmail={Uri.EscapeDataString(request.ToEmail)}")
            + (string.IsNullOrWhiteSpace(request.TemplateCode) ? "" : $"&TemplateCode={Uri.EscapeDataString(request.TemplateCode)}")
            + (string.IsNullOrWhiteSpace(request.SourceModule) ? "" : $"&SourceModule={Uri.EscapeDataString(request.SourceModule)}")
            + (string.IsNullOrWhiteSpace(request.Status) ? "" : $"&Status={Uri.EscapeDataString(request.Status)}")
            + (request.FromDate is null ? "" : $"&FromDate={request.FromDate:yyyy-MM-dd}")
            + (request.ToDate is null ? "" : $"&ToDate={request.ToDate:yyyy-MM-dd}");

        return apiClient.GetAsync<PagedResult<EmailHistoryListItemDto>>(query);
    }

    public Task<ApiResult<EmailHistoryDetailDto>> GetByIdAsync(long id) =>
        apiClient.GetAsync<EmailHistoryDetailDto>($"api/settings/email-history/{id}");

    public Task<ApiResult<EmailHistoryAdjacentDto>> GetAdjacentAsync(long id) =>
        apiClient.GetAsync<EmailHistoryAdjacentDto>($"api/settings/email-history/{id}/adjacent");
}
