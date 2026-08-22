using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.MasterData;

namespace Sugentra.ERP.UI.Services.MasterData;

public class PriceListApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/master-data/price-lists";

    public Task<ApiResult<IReadOnlyList<PriceListResponse>>> GetAllAsync() =>
        apiClient.GetAsync<IReadOnlyList<PriceListResponse>>(BaseRoute);

    public Task<ApiResult<PagedResult<PriceListListItemDto>>> GetPagedAsync(PriceListListRequest request)
    {
        var query = $"{BaseRoute}/paged?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.Keyword) ? "" : $"&Keyword={Uri.EscapeDataString(request.Keyword)}");

        return apiClient.GetAsync<PagedResult<PriceListListItemDto>>(query);
    }

    public Task<ApiResult<PriceListAdjacentDto>> GetAdjacentAsync(long id) =>
        apiClient.GetAsync<PriceListAdjacentDto>($"{BaseRoute}/{id}/adjacent");

    public Task<ApiResult<PriceListResponse>> GetByIdAsync(long id) =>
        apiClient.GetAsync<PriceListResponse>($"{BaseRoute}/{id}");

    public Task<ApiResult<PriceListResponse>> CreateAsync(CreatePriceListRequest request) =>
        apiClient.PostAsync<PriceListResponse>(BaseRoute, request);

    public Task<ApiResult<PriceListResponse>> UpdateAsync(long id, UpdatePriceListRequest request) =>
        apiClient.PutAsync<PriceListResponse>($"{BaseRoute}/{id}", request);

    public Task<ApiResult<bool>> DeleteAsync(long id) => apiClient.DeleteAsync($"{BaseRoute}/{id}");
}
