using Sugentra.ERP.Api.Modules.Inventory.UseCases;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Inventory.Services;

// Bridges the generic Approvals engine back into Goods Receipt posting.
public class GoodsReceiptApprovalHandler(GoodsReceiptUseCase goodsReceiptUseCase) : IApprovalDocumentHandler
{
    public string DocumentType => "GoodsReceipt";

    public Task OnApprovedAsync(long documentId) => goodsReceiptUseCase.CompleteApprovedPostAsync(documentId);

    public Task OnRejectedAsync(long documentId, string? reason) => goodsReceiptUseCase.RejectPostAsync(documentId);

    public Task OnLevelChangedAsync(long documentId, string levelName, int levelNumber, int totalLevelCount) =>
        goodsReceiptUseCase.SetWaitingApprovalLevelAsync(documentId, levelName);
}
