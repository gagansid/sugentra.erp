using Sugentra.ERP.Api.Modules.Approvals.UseCases;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Approvals.Services;

// Thin adapter exposing the engine via Shared/Contracts so other modules never reference Modules/Approvals directly.
public class ApprovalService(ApprovalRequestUseCase requestUseCase) : IApprovalService
{
    public Task<ApprovalSubmissionResult> SubmitForApprovalAsync(ApprovalSubmissionRequest request) => requestUseCase.SubmitAsync(request);

    public Task<ApprovalStatusDto?> GetStatusAsync(string documentType, long documentId) => requestUseCase.GetStatusAsync(documentType, documentId);
}
