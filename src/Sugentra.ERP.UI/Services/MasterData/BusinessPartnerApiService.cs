using Sugentra.ERP.UI.Models;
using Sugentra.ERP.UI.Models.MasterData;

namespace Sugentra.ERP.UI.Services.MasterData;

public class BusinessPartnerApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/master-data/business-partners";

    public Task<ApiResult<IReadOnlyList<BusinessPartnerResponse>>> GetAllAsync() =>
        apiClient.GetAsync<IReadOnlyList<BusinessPartnerResponse>>(BaseRoute);

    public Task<ApiResult<PagedResult<BusinessPartnerListItemDto>>> GetPagedAsync(BusinessPartnerListRequest request)
    {
        var query = $"{BaseRoute}/paged?Page={request.Page}&PageSize={request.PageSize}"
            + (string.IsNullOrWhiteSpace(request.Keyword) ? "" : $"&Keyword={Uri.EscapeDataString(request.Keyword)}");

        return apiClient.GetAsync<PagedResult<BusinessPartnerListItemDto>>(query);
    }

    public Task<ApiResult<BusinessPartnerAdjacentDto>> GetAdjacentAsync(long id) =>
        apiClient.GetAsync<BusinessPartnerAdjacentDto>($"{BaseRoute}/{id}/adjacent");

    public Task<ApiResult<BusinessPartnerResponse>> GetByIdAsync(long id) =>
        apiClient.GetAsync<BusinessPartnerResponse>($"{BaseRoute}/{id}");

    public Task<ApiResult<BusinessPartnerResponse>> CreateAsync(CreateBusinessPartnerRequest request) =>
        apiClient.PostAsync<BusinessPartnerResponse>(BaseRoute, request);

    public Task<ApiResult<BusinessPartnerResponse>> UpdateAsync(long id, UpdateBusinessPartnerRequest request) =>
        apiClient.PutAsync<BusinessPartnerResponse>($"{BaseRoute}/{id}", request);

    public Task<ApiResult<bool>> DeleteAsync(long id) => apiClient.DeleteAsync($"{BaseRoute}/{id}");
}
