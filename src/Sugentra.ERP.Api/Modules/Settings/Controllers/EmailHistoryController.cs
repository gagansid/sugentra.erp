using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Queries;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/email-history")]
[Authorize]
public class EmailHistoryController(EmailHistoryQuery emailHistoryQuery) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "EmailHistory_View")]
    public async Task<IActionResult> GetPaged([FromQuery] EmailHistoryListRequest request) =>
        Success(await emailHistoryQuery.GetPagedAsync(request));

    [HttpGet("{id:long}")]
    [Authorize(Policy = "EmailHistory_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entry = await emailHistoryQuery.GetByIdAsync(id);
        return entry is null ? Failure("Email history entry not found.", StatusCodes.Status404NotFound) : Success(entry);
    }

    [HttpGet("{id:long}/adjacent")]
    [Authorize(Policy = "EmailHistory_View")]
    public async Task<IActionResult> GetAdjacent(long id) =>
        Success(await emailHistoryQuery.GetAdjacentAsync(id));
}
