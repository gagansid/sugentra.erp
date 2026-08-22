using Sugentra.ERP.UI.Models.Settings;

namespace Sugentra.ERP.UI.Services.Settings;

/// <summary>Manages the module catalog (admin CRUD) plus the current-user-scoped active list for the hub grid.</summary>
public class ModuleApiService(ApiClient apiClient) : CrudApiService<Module>(apiClient, "api/settings/modules")
{
    private readonly ApiClient _apiClient = apiClient;

    public Task<ApiResult<IReadOnlyList<Module>>> GetActiveForCurrentUserAsync() =>
        _apiClient.GetAsync<IReadOnlyList<Module>>("api/settings/modules/active");
}
