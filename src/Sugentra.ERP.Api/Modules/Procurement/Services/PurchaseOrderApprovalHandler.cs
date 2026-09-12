using Sugentra.ERP.Api.Modules.Procurement.UseCases;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Procurement.Services;

public class PurchaseOrderApprovalHandler(PurchaseOrderUseCase purchaseOrderUseCase) : IApprovalDocumentHandler
{
    public string DocumentType => "PurchaseOrder";

    public Task OnApprovedAsync(long documentId) => purchaseOrderUseCase.CompleteApprovedAsync(documentId);

    public Task OnRejectedAsync(long documentId, string? reason) => purchaseOrderUseCase.RejectAsync(documentId);

    public Task OnLevelChangedAsync(long documentId, string levelName, int levelNumber, int totalLevelCount)
        => purchaseOrderUseCase.SetWaitingApprovalLevelAsync(documentId, levelName);
}
