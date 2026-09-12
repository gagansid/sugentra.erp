using Sugentra.ERP.Api.Modules.Procurement.UseCases;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Procurement.Services;

public class PurchaseRequisitionApprovalHandler(PurchaseRequisitionUseCase purchaseRequisitionUseCase) : IApprovalDocumentHandler
{
    public string DocumentType => "PurchaseRequisition";

    public Task OnApprovedAsync(long documentId) => purchaseRequisitionUseCase.CompleteApprovedAsync(documentId);

    public Task OnRejectedAsync(long documentId, string? reason) => purchaseRequisitionUseCase.RejectAsync(documentId);

    public Task OnLevelChangedAsync(long documentId, string levelName, int levelNumber, int totalLevelCount)
        => purchaseRequisitionUseCase.SetWaitingApprovalLevelAsync(documentId, levelName);
}
