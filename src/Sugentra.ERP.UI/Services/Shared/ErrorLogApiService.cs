using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Shared;

namespace Sugentra.ERP.UI.Services.Shared;

public class ErrorLogApiService(ApiClient apiClient)
{
    public Task<ApiResult<object?>> ReportAsync(ErrorLogEntry entry) =>
        apiClient.PostAsync<object?>("api/error-logs", entry);

    public Task<ApiResult<PagedResult<ErrorLogListItemDto>>> GetPagedAsync(ErrorLogListRequest request)
    {
        var query = $"api/error-logs?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.Source) ? "" : $"&Source={Uri.EscapeDataString(request.Source)}")
            + (string.IsNullOrWhiteSpace(request.Search) ? "" : $"&Search={Uri.EscapeDataString(request.Search)}")
            + (request.FromDate is null ? "" : $"&FromDate={request.FromDate:yyyy-MM-dd}")
            + (request.ToDate is null ? "" : $"&ToDate={request.ToDate:yyyy-MM-dd}");

        return apiClient.GetAsync<PagedResult<ErrorLogListItemDto>>(query);
    }

    public Task<ApiResult<ErrorLogDetailDto>> GetByIdAsync(long id) =>
        apiClient.GetAsync<ErrorLogDetailDto>($"api/error-logs/{id}");

    public Task<ApiResult<object?>> UpdateStatusAsync(long id, string status, string? remarks) =>
        apiClient.PutAsync<object?>($"api/error-logs/{id}/status", new UpdateErrorLogStatusRequest(status, remarks));

    public Task<ApiResult<ErrorLogAdjacentDto>> GetAdjacentAsync(long id) =>
        apiClient.GetAsync<ErrorLogAdjacentDto>($"api/error-logs/{id}/adjacent");

    public Task<ApiResult<object?>> SendEmailAsync(long id, string toEmail) =>
        apiClient.PostAsync<object?>($"api/error-logs/{id}/send-email", new SendErrorLogEmailRequest(toEmail));

    public Task<ApiResult<string?>> GetDefaultNotificationEmailAsync() =>
        apiClient.GetAsync<string?>("api/error-logs/default-notification-email");
}
