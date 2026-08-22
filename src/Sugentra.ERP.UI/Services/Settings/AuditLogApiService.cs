using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Settings;

namespace Sugentra.ERP.UI.Services.Settings;

public class AuditLogApiService(ApiClient apiClient)
{
    public Task<ApiResult<PagedResult<AuditLogListItemDto>>> GetPagedAsync(AuditLogListRequest request)
    {
        var query = $"api/settings/audit-logs?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.TableName) ? "" : $"&TableName={Uri.EscapeDataString(request.TableName)}")
            + (request.RecordId is null ? "" : $"&RecordId={request.RecordId}")
            + (request.ChangedByUserId is null ? "" : $"&ChangedByUserId={request.ChangedByUserId}")
            + (string.IsNullOrWhiteSpace(request.Action) ? "" : $"&Action={Uri.EscapeDataString(request.Action)}")
            + (string.IsNullOrWhiteSpace(request.Module) ? "" : $"&Module={Uri.EscapeDataString(request.Module)}")
            + (string.IsNullOrWhiteSpace(request.Menu) ? "" : $"&Menu={Uri.EscapeDataString(request.Menu)}")
            + (request.FromDate is null ? "" : $"&FromDate={request.FromDate:yyyy-MM-dd}")
            + (request.ToDate is null ? "" : $"&ToDate={request.ToDate:yyyy-MM-dd}");

        return apiClient.GetAsync<PagedResult<AuditLogListItemDto>>(query);
    }

    public Task<ApiResult<AuditLogFilterOptionsDto>> GetFilterOptionsAsync() =>
        apiClient.GetAsync<AuditLogFilterOptionsDto>("api/settings/audit-logs/filter-options");

    public Task<ApiResult<AuditLogListItemDto>> GetByIdAsync(long id) =>
        apiClient.GetAsync<AuditLogListItemDto>($"api/settings/audit-logs/{id}");

    public Task<ApiResult<AuditLogAdjacentDto>> GetAdjacentAsync(long id, string? tableName = null, long? recordId = null, long? changedByUserId = null)
    {
        var query = $"api/settings/audit-logs/{id}/adjacent"
            + (string.IsNullOrWhiteSpace(tableName) ? "" : $"?tableName={Uri.EscapeDataString(tableName)}")
            + (recordId is null ? "" : $"{(string.IsNullOrWhiteSpace(tableName) ? "?" : "&")}recordId={recordId}")
            + (changedByUserId is null ? "" : $"{(string.IsNullOrWhiteSpace(tableName) && recordId is null ? "?" : "&")}changedByUserId={changedByUserId}");

        return apiClient.GetAsync<AuditLogAdjacentDto>(query);
    }
}
