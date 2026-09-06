using Sugentra.ERP.Api.Modules.Inventory.UseCases;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Inventory.Services;

// Bridges the generic Approvals engine back into Stock Mutation posting.
public class StockMutationApprovalHandler(StockMutationUseCase stockMutationUseCase) : IApprovalDocumentHandler
{
    public string DocumentType => "StockMutation";

    public Task OnApprovedAsync(long documentId) => stockMutationUseCase.CompleteApprovedPostAsync(documentId);

    public Task OnRejectedAsync(long documentId, string? reason) => stockMutationUseCase.RejectPostAsync(documentId);

    public Task OnLevelChangedAsync(long documentId, string levelName, int levelNumber, int totalLevelCount) =>
        stockMutationUseCase.SetWaitingApprovalLevelAsync(documentId, levelName);
}
