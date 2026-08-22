using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Identity;

namespace Sugentra.ERP.UI.Services.Identity;

public class RoleApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/identity/roles";

    public Task<ApiResult<PagedResult<RoleListItemDto>>> GetPagedAsync(RoleListRequest request)
    {
        var query = $"{BaseRoute}?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.Name) ? "" : $"&Name={Uri.EscapeDataString(request.Name)}");

        return apiClient.GetAsync<PagedResult<RoleListItemDto>>(query);
    }

    public Task<ApiResult<RoleResponse>> CreateAsync(CreateRoleRequest request) =>
        apiClient.PostAsync<RoleResponse>(BaseRoute, request);

    public Task<ApiResult<RoleResponse>> GetByIdAsync(long id) =>
        apiClient.GetAsync<RoleResponse>($"{BaseRoute}/{id}");

    public Task<ApiResult<RoleResponse>> UpdateAsync(long id, UpdateRoleRequest request) =>
        apiClient.PutAsync<RoleResponse>($"{BaseRoute}/{id}", request);

    public Task<ApiResult<bool>> DeleteAsync(long id) => apiClient.DeleteAsync($"{BaseRoute}/{id}");

    public Task<ApiResult<bool>> AssignPermissionAsync(long id, long permissionId) => apiClient.PostAsync<bool>($"{BaseRoute}/{id}/permissions/{permissionId}");

    public Task<ApiResult<bool>> RevokePermissionAsync(long id, long permissionId) => apiClient.DeleteAsync($"{BaseRoute}/{id}/permissions/{permissionId}");

    public Task<ApiResult<PagedResult<RolePermissionListItemDto>>> GetPermissionsAsync(long id, RolePermissionListRequest request)
    {
        var query = $"{BaseRoute}/{id}/permissions?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.Module) ? "" : $"&Module={Uri.EscapeDataString(request.Module)}")
            + (string.IsNullOrWhiteSpace(request.Code) ? "" : $"&Code={Uri.EscapeDataString(request.Code)}")
            + (string.IsNullOrWhiteSpace(request.Description) ? "" : $"&Description={Uri.EscapeDataString(request.Description)}")
            + (request.IsAssigned is null ? "" : $"&IsAssigned={request.IsAssigned}");

        return apiClient.GetAsync<PagedResult<RolePermissionListItemDto>>(query);
    }
}
