using Sugentra.ERP.UI.Models.Approvals;
using Sugentra.ERP.UI.Models.Procurement;

namespace Sugentra.ERP.UI.Services.Procurement;

public class PurchaseOrderApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/procurement/purchase-orders";

    public Task<ApiResult<IReadOnlyList<PurchaseOrderResponse>>> GetAllAsync() => apiClient.GetAsync<IReadOnlyList<PurchaseOrderResponse>>(BaseRoute);

    public Task<ApiResult<PurchaseOrderResponse>> GetByIdAsync(long id) => apiClient.GetAsync<PurchaseOrderResponse>($"{BaseRoute}/{id}");

    public Task<ApiResult<PurchaseOrderResponse>> CreateAsync(CreatePurchaseOrderRequest request) => apiClient.PostAsync<PurchaseOrderResponse>(BaseRoute, request);

    public Task<ApiResult<PurchaseOrderResponse>> UpdateAsync(long id, UpdatePurchaseOrderRequest request) => apiClient.PutAsync<PurchaseOrderResponse>($"{BaseRoute}/{id}", request);

    public Task<ApiResult<PurchaseOrderResponse>> SubmitAsync(long id) => apiClient.PostAsync<PurchaseOrderResponse>($"{BaseRoute}/{id}/submit", new { });

    public Task<ApiResult<bool>> DeleteAsync(long id) => apiClient.DeleteAsync($"{BaseRoute}/{id}");

    public Task<ApiResult<PurchaseOrderAdjacentDto>> GetAdjacentAsync(long id) => apiClient.GetAsync<PurchaseOrderAdjacentDto>($"{BaseRoute}/{id}/adjacent");

    public Task<ApiResult<IReadOnlyList<ApprovalHistoryEntry>>> GetApprovalHistoryAsync(long id) => apiClient.GetAsync<IReadOnlyList<ApprovalHistoryEntry>>($"{BaseRoute}/{id}/approval-history");
}
