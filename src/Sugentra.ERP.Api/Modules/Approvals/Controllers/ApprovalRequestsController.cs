using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Approvals.Dtos;
using Sugentra.ERP.Api.Modules.Approvals.UseCases;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Approvals.Controllers;

// "My Approvals" inbox — pending requests waiting on the current user, plus request detail/history and act.
[Route("api/approvals/requests")]
[Authorize]
public class ApprovalRequestsController(ApprovalRequestUseCase useCase, ICurrentUserService currentUserService) : ApiControllerBase
{
    [HttpGet("inbox")]
    [Authorize(Policy = "ApprovalRequest_View")]
    public async Task<IActionResult> GetInbox() => Success(await useCase.GetInboxAsync(currentUserService.UserId ?? 0));

    [HttpGet("{id:long}")]
    [Authorize(Policy = "ApprovalRequest_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var response = await useCase.GetDetailAsync(id);
        return response is null ? Failure("Approval request not found.", StatusCodes.Status404NotFound) : Success(response);
    }

    [HttpPost("{id:long}/act")]
    [Authorize(Policy = "ApprovalRequest_Act")]
    public async Task<IActionResult> Act(long id, [FromBody] ApprovalActionRequest request)
    {
        var result = await useCase.ActAsync(id, currentUserService.UserId ?? 0, request.Approve, request.Comment);
        return result.IsSuccess
            ? Success(result.Value, request.Approve ? "Approved successfully." : "Rejected successfully.")
            : Failure(result.Error!, StatusCodes.Status400BadRequest);
    }
}
