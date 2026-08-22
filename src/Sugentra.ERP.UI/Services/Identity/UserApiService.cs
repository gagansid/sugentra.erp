using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.Identity;

namespace Sugentra.ERP.UI.Services.Identity;

public class UserApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/identity/users";

    public Task<ApiResult<PagedResult<UserListItemDto>>> GetPagedAsync(UserListRequest request)
    {
        var query = $"{BaseRoute}?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.Username) ? "" : $"&Username={Uri.EscapeDataString(request.Username)}")
            + (string.IsNullOrWhiteSpace(request.Email) ? "" : $"&Email={Uri.EscapeDataString(request.Email)}")
            + (string.IsNullOrWhiteSpace(request.FullName) ? "" : $"&FullName={Uri.EscapeDataString(request.FullName)}");

        return apiClient.GetAsync<PagedResult<UserListItemDto>>(query);
    }

    public Task<ApiResult<UserResponse>> CreateAsync(CreateUserRequest request) =>
        apiClient.PostAsync<UserResponse>(BaseRoute, request);

    public Task<ApiResult<UserResponse>> GetByIdAsync(long id) =>
        apiClient.GetAsync<UserResponse>($"{BaseRoute}/{id}");

    public Task<ApiResult<UserResponse>> UpdateAsync(long id, UpdateUserRequest request) =>
        apiClient.PutAsync<UserResponse>($"{BaseRoute}/{id}", request);

    public Task<ApiResult<bool>> DeactivateAsync(long id) => apiClient.DeleteAsync($"{BaseRoute}/{id}");

    public Task<ApiResult<bool>> UnlockAsync(long id) => apiClient.PostAsync<bool>($"{BaseRoute}/{id}/unlock");

    public Task<ApiResult<bool>> ChangePasswordAsync(long id, ChangePasswordRequest request) =>
        apiClient.PostAsync<bool>($"{BaseRoute}/{id}/change-password", request);

    public Task<ApiResult<bool>> AssignRoleAsync(long id, long roleId) => apiClient.PostAsync<bool>($"{BaseRoute}/{id}/roles/{roleId}");

    public Task<ApiResult<bool>> RevokeRoleAsync(long id, long roleId) => apiClient.DeleteAsync($"{BaseRoute}/{id}/roles/{roleId}");

    public Task<ApiResult<UserRolesPermissionsResponse>> GetRolesAndPermissionsAsync(long id) =>
        apiClient.GetAsync<UserRolesPermissionsResponse>($"{BaseRoute}/{id}/roles-permissions");

    public Task<ApiResult<bool>> SetPermissionOverrideAsync(long id, long permissionId, bool isAllowed) =>
        apiClient.PutAsync<bool>($"{BaseRoute}/{id}/permissions/{permissionId}", new { IsAllowed = isAllowed });

    public Task<ApiResult<bool>> RemovePermissionOverrideAsync(long id, long permissionId) =>
        apiClient.DeleteAsync($"{BaseRoute}/{id}/permissions/{permissionId}");

    public Task<ApiResult<PagedResult<UserSessionDto>>> GetSessionsAsync(long id, int page = 1, int pageSize = 10) =>
        apiClient.GetAsync<PagedResult<UserSessionDto>>($"{BaseRoute}/{id}/sessions?page={page}&pageSize={pageSize}");

    public Task<ApiResult<bool>> RevokeSessionAsync(long id, long sessionId) =>
        apiClient.PostAsync<bool>($"{BaseRoute}/{id}/sessions/{sessionId}/revoke");
}
