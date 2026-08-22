using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Identity;

namespace Sugentra.ERP.UI.Services.Identity;

public class PermissionApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/identity/permissions";

    public Task<ApiResult<PagedResult<PermissionListItemDto>>> GetPagedAsync(PermissionListRequest request)
    {
        var query = $"{BaseRoute}?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.Code) ? "" : $"&Code={Uri.EscapeDataString(request.Code)}")
            + (string.IsNullOrWhiteSpace(request.Module) ? "" : $"&Module={Uri.EscapeDataString(request.Module)}");

        return apiClient.GetAsync<PagedResult<PermissionListItemDto>>(query);
    }
}
