using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.MasterData;

namespace Sugentra.ERP.UI.Services.MasterData;

public class BillOfMaterialApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/master-data/bill-of-materials";

    public Task<ApiResult<IReadOnlyList<BillOfMaterialResponse>>> GetAllAsync() =>
        apiClient.GetAsync<IReadOnlyList<BillOfMaterialResponse>>(BaseRoute);

    public Task<ApiResult<PagedResult<BillOfMaterialListItemDto>>> GetPagedAsync(BillOfMaterialListRequest request)
    {
        var query = $"{BaseRoute}/paged?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.Keyword) ? "" : $"&Keyword={Uri.EscapeDataString(request.Keyword)}");

        return apiClient.GetAsync<PagedResult<BillOfMaterialListItemDto>>(query);
    }

    public Task<ApiResult<BillOfMaterialAdjacentDto>> GetAdjacentAsync(long id) =>
        apiClient.GetAsync<BillOfMaterialAdjacentDto>($"{BaseRoute}/{id}/adjacent");

    public Task<ApiResult<BillOfMaterialResponse>> GetByIdAsync(long id) =>
        apiClient.GetAsync<BillOfMaterialResponse>($"{BaseRoute}/{id}");

    public Task<ApiResult<BillOfMaterialResponse>> CreateAsync(CreateBillOfMaterialRequest request) =>
        apiClient.PostAsync<BillOfMaterialResponse>(BaseRoute, request);

    public Task<ApiResult<BillOfMaterialResponse>> UpdateAsync(long id, UpdateBillOfMaterialRequest request) =>
        apiClient.PutAsync<BillOfMaterialResponse>($"{BaseRoute}/{id}", request);

    public Task<ApiResult<bool>> DeleteAsync(long id) => apiClient.DeleteAsync($"{BaseRoute}/{id}");
}
