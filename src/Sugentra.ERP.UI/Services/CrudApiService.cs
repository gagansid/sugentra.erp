namespace Sugentra.ERP.UI.Services;

/// <summary>Generic client for the simple CRUD API pattern used by MasterData/Settings reference-data
/// controllers (GetAll/GetById/Create/Update/Delete over a raw entity, no paging) — mirrors CrudUseCase&lt;T&gt;
/// on the API side.</summary>
public class CrudApiService<T>(ApiClient apiClient, string baseRoute)
{
    protected ApiClient ApiClient => apiClient;

    public Task<ApiResult<IReadOnlyList<T>>> GetAllAsync() => apiClient.GetAsync<IReadOnlyList<T>>(baseRoute);

    public Task<ApiResult<T>> GetByIdAsync(long id) => apiClient.GetAsync<T>($"{baseRoute}/{id}");

    public Task<ApiResult<T>> CreateAsync(T entity) => apiClient.PostAsync<T>(baseRoute, entity);

    public Task<ApiResult<T>> UpdateAsync(long id, T entity) => apiClient.PutAsync<T>($"{baseRoute}/{id}", entity);

    public Task<ApiResult<bool>> DeleteAsync(long id) => apiClient.DeleteAsync($"{baseRoute}/{id}");
}
