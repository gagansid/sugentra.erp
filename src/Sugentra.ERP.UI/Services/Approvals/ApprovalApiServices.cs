using Sugentra.ERP.UI.Models.Approvals;

namespace Sugentra.ERP.UI.Services.Approvals;

public class ApprovalFlowApiService(ApiClient apiClient) : CrudApiService<ApprovalFlowDefinitionResponse>(apiClient, "api/approvals/flows")
{
    public Task<ApiResult<ApprovalFlowDefinitionResponse>> CreateAsync(SaveApprovalFlowDefinitionRequest request) =>
        ApiClient.PostAsync<ApprovalFlowDefinitionResponse>("api/approvals/flows", request);

    public Task<ApiResult<ApprovalFlowDefinitionResponse>> UpdateAsync(long id, SaveApprovalFlowDefinitionRequest request) =>
        ApiClient.PutAsync<ApprovalFlowDefinitionResponse>($"api/approvals/flows/{id}", request);
}

public class ApprovalRequestApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/approvals/requests";

    public Task<ApiResult<IReadOnlyList<ApprovalInboxItem>>> GetInboxAsync() =>
        apiClient.GetAsync<IReadOnlyList<ApprovalInboxItem>>($"{BaseRoute}/inbox");

    public Task<ApiResult<ApprovalRequestResponse>> GetByIdAsync(long id) =>
        apiClient.GetAsync<ApprovalRequestResponse>($"{BaseRoute}/{id}");

    public Task<ApiResult<ApprovalRequestResponse>> ActAsync(long id, ApprovalActionRequest request) =>
        apiClient.PostAsync<ApprovalRequestResponse>($"{BaseRoute}/{id}/act", request);
}

public class ApprovalRoleCategoryApiService(ApiClient apiClient) : CrudApiService<ApprovalRoleCategoryResponse>(apiClient, "api/approvals/role-categories");

