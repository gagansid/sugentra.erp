using Sugentra.ERP.UI.Models.Layout;
using Sugentra.ERP.UI.Models.Settings;

namespace Sugentra.ERP.UI.Services.Settings;

public class MenuApiService(ApiClient apiClient) : CrudApiService<Menu>(apiClient, "api/settings/menus")
{
    private readonly ApiClient _apiClient = apiClient;

    public Task<ApiResult<IReadOnlyList<MenuModuleGroupDto>>> GetActiveForCurrentUserAsync() =>
        _apiClient.GetAsync<IReadOnlyList<MenuModuleGroupDto>>("api/settings/menus/active");
}
