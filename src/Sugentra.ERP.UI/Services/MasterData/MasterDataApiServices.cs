using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.MasterData;

namespace Sugentra.ERP.UI.Services.MasterData;

public class ItemApiService(ApiClient apiClient) : CrudApiService<Item>(apiClient, "api/master-data/items")
{
    public Task<ApiResult<PagedResult<ItemListItemDto>>> GetPagedAsync(ItemListRequest request)
    {
        var query = $"api/master-data/items/paged?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.Keyword) ? "" : $"&Keyword={Uri.EscapeDataString(request.Keyword)}");

        return ApiClient.GetAsync<PagedResult<ItemListItemDto>>(query);
    }

    public Task<ApiResult<ItemAdjacentDto>> GetAdjacentAsync(long id) =>
        ApiClient.GetAsync<ItemAdjacentDto>($"api/master-data/items/{id}/adjacent");
}
