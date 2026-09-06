using Sugentra.ERP.Api.Modules.Inventory.UseCases;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Inventory.Services;

// Bridges the generic Approvals engine back into Stock Opname posting.
public class StockOpnameApprovalHandler(StockOpnameUseCase stockOpnameUseCase) : IApprovalDocumentHandler
{
    public string DocumentType => "StockOpname";

    public Task OnApprovedAsync(long documentId) => stockOpnameUseCase.CompleteApprovedPostAsync(documentId);

    public Task OnRejectedAsync(long documentId, string? reason) => stockOpnameUseCase.RejectPostAsync(documentId);

    public Task OnLevelChangedAsync(long documentId, string levelName, int levelNumber, int totalLevelCount) =>
        stockOpnameUseCase.SetWaitingApprovalLevelAsync(documentId, levelName);
}
