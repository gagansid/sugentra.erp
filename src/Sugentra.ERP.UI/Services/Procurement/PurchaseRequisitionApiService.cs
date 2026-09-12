using Sugentra.ERP.UI.Models.Approvals;
using Sugentra.ERP.UI.Models.Procurement;

namespace Sugentra.ERP.UI.Services.Procurement;

public class PurchaseRequisitionApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/procurement/purchase-requisitions";

    public Task<ApiResult<IReadOnlyList<PurchaseRequisitionResponse>>> GetAllAsync() => apiClient.GetAsync<IReadOnlyList<PurchaseRequisitionResponse>>(BaseRoute);

    public Task<ApiResult<PurchaseRequisitionResponse>> GetByIdAsync(long id) => apiClient.GetAsync<PurchaseRequisitionResponse>($"{BaseRoute}/{id}");

    public Task<ApiResult<PurchaseRequisitionResponse>> CreateAsync(CreatePurchaseRequisitionRequest request) => apiClient.PostAsync<PurchaseRequisitionResponse>(BaseRoute, request);

    public Task<ApiResult<PurchaseRequisitionResponse>> UpdateAsync(long id, UpdatePurchaseRequisitionRequest request) => apiClient.PutAsync<PurchaseRequisitionResponse>($"{BaseRoute}/{id}", request);

    public Task<ApiResult<PurchaseRequisitionResponse>> SubmitAsync(long id) => apiClient.PostAsync<PurchaseRequisitionResponse>($"{BaseRoute}/{id}/submit", new { });

    public Task<ApiResult<bool>> DeleteAsync(long id) => apiClient.DeleteAsync($"{BaseRoute}/{id}");

    public Task<ApiResult<PurchaseRequisitionAdjacentDto>> GetAdjacentAsync(long id) => apiClient.GetAsync<PurchaseRequisitionAdjacentDto>($"{BaseRoute}/{id}/adjacent");

    public Task<ApiResult<IReadOnlyList<ApprovalHistoryEntry>>> GetApprovalHistoryAsync(long id) => apiClient.GetAsync<IReadOnlyList<ApprovalHistoryEntry>>($"{BaseRoute}/{id}/approval-history");
}
