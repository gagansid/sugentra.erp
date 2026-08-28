using Sugentra.ERP.UI.Models.Inventory;
using Sugentra.ERP.UI.Models.Approvals;

namespace Sugentra.ERP.UI.Services.Inventory;

public class BatchApiService(ApiClient apiClient) : CrudApiService<Batch>(apiClient, "api/inventory/batches")
{
    public Task<ApiResult<bool>> HasStockAsync(long id) => ApiClient.GetAsync<bool>($"api/inventory/batches/{id}/has-stock");
}

public class LandedCostAllocationApiService(ApiClient apiClient) : CrudApiService<LandedCostAllocation>(apiClient, "api/inventory/landed-cost-allocations");

public class GoodsReceiptApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/inventory/goods-receipts";

    public Task<ApiResult<IReadOnlyList<GoodsReceiptResponse>>> GetAllAsync() => apiClient.GetAsync<IReadOnlyList<GoodsReceiptResponse>>(BaseRoute);

    public Task<ApiResult<GoodsReceiptResponse>> GetByIdAsync(long id) => apiClient.GetAsync<GoodsReceiptResponse>($"{BaseRoute}/{id}");

    public Task<ApiResult<GoodsReceiptResponse>> CreateAsync(CreateGoodsReceiptRequest request) => apiClient.PostAsync<GoodsReceiptResponse>(BaseRoute, request);

    public Task<ApiResult<GoodsReceiptResponse>> UpdateAsync(long id, UpdateGoodsReceiptRequest request) => apiClient.PutAsync<GoodsReceiptResponse>($"{BaseRoute}/{id}", request);

    public Task<ApiResult<GoodsReceiptResponse>> PostGoodsReceiptAsync(long id) => apiClient.PostAsync<GoodsReceiptResponse>($"{BaseRoute}/{id}/post", new { });

    public Task<ApiResult<bool>> DeleteAsync(long id) => apiClient.DeleteAsync($"{BaseRoute}/{id}");

    public Task<ApiResult<GoodsReceiptAdjacentDto>> GetAdjacentAsync(long id) => apiClient.GetAsync<GoodsReceiptAdjacentDto>($"{BaseRoute}/{id}/adjacent");

    public Task<ApiResult<IReadOnlyList<ApprovalHistoryEntry>>> GetApprovalHistoryAsync(long id) => apiClient.GetAsync<IReadOnlyList<ApprovalHistoryEntry>>($"{BaseRoute}/{id}/approval-history");
}

public class QuarantineHoldApiService(ApiClient apiClient) : CrudApiService<QuarantineHold>(apiClient, "api/inventory/quarantine-holds");

/// <summary>Read-only: stock balances are a system-computed snapshot, never edited directly from the UI.</summary>
public class StockBalanceApiService(ApiClient apiClient)
{
    public Task<ApiResult<IReadOnlyList<StockBalance>>> GetAllAsync() => apiClient.GetAsync<IReadOnlyList<StockBalance>>("api/inventory/stock-balances");

    public Task<ApiResult<StockBalance>> GetByIdAsync(long id) => apiClient.GetAsync<StockBalance>($"api/inventory/stock-balances/{id}");
}

/// <summary>Read-only: the stock ledger (kartu stok) is an immutable append-only audit trail.</summary>
public class StockLedgerApiService(ApiClient apiClient)
{
    public Task<ApiResult<IReadOnlyList<StockLedger>>> GetAllAsync() => apiClient.GetAsync<IReadOnlyList<StockLedger>>("api/inventory/stock-ledgers");

    public Task<ApiResult<StockLedger>> GetByIdAsync(long id) => apiClient.GetAsync<StockLedger>($"api/inventory/stock-ledgers/{id}");
}

public class StockMutationApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/inventory/stock-mutations";

    public Task<ApiResult<IReadOnlyList<StockMutationResponse>>> GetAllAsync() => apiClient.GetAsync<IReadOnlyList<StockMutationResponse>>(BaseRoute);

    public Task<ApiResult<StockMutationResponse>> GetByIdAsync(long id) => apiClient.GetAsync<StockMutationResponse>($"{BaseRoute}/{id}");

    public Task<ApiResult<StockMutationResponse>> CreateAsync(CreateStockMutationRequest request) => apiClient.PostAsync<StockMutationResponse>(BaseRoute, request);

    public Task<ApiResult<StockMutationResponse>> UpdateAsync(long id, UpdateStockMutationRequest request) => apiClient.PutAsync<StockMutationResponse>($"{BaseRoute}/{id}", request);

    public Task<ApiResult<StockMutationResponse>> ApproveAsync(long id) => apiClient.PostAsync<StockMutationResponse>($"{BaseRoute}/{id}/approve", new { });

    public Task<ApiResult<StockMutationResponse>> CompleteAsync(long id) => apiClient.PostAsync<StockMutationResponse>($"{BaseRoute}/{id}/complete", new { });

    public Task<ApiResult<bool>> DeleteAsync(long id) => apiClient.DeleteAsync($"{BaseRoute}/{id}");
}

public class StockOpnameApiService(ApiClient apiClient)
{
    private const string BaseRoute = "api/inventory/stock-opnames";

    public Task<ApiResult<IReadOnlyList<StockOpnameResponse>>> GetAllAsync() => apiClient.GetAsync<IReadOnlyList<StockOpnameResponse>>(BaseRoute);

    public Task<ApiResult<StockOpnameResponse>> GetByIdAsync(long id) => apiClient.GetAsync<StockOpnameResponse>($"{BaseRoute}/{id}");

    public Task<ApiResult<StockOpnameResponse>> CreateAsync(CreateStockOpnameRequest request) => apiClient.PostAsync<StockOpnameResponse>(BaseRoute, request);

    public Task<ApiResult<StockOpnameResponse>> UpdateAsync(long id, UpdateStockOpnameRequest request) => apiClient.PutAsync<StockOpnameResponse>($"{BaseRoute}/{id}", request);

    public Task<ApiResult<StockOpnameResponse>> SubmitAsync(long id) => apiClient.PostAsync<StockOpnameResponse>($"{BaseRoute}/{id}/submit", new { });

    public Task<ApiResult<StockOpnameResponse>> ApproveAsync(long id) => apiClient.PostAsync<StockOpnameResponse>($"{BaseRoute}/{id}/approve", new { });

    public Task<ApiResult<bool>> DeleteAsync(long id) => apiClient.DeleteAsync($"{BaseRoute}/{id}");
}
