using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Modules.Settings.Queries;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Controllers;

[Route("api/settings/audit-logs")]
[Authorize]
public class AuditLogsController(AuditLogQuery auditLogQuery) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "AuditLog_View")]
    public async Task<IActionResult> GetPaged([FromQuery] AuditLogListRequest request) =>
        Success(await auditLogQuery.GetPagedAsync(request));

    [HttpGet("filter-options")]
    [Authorize(Policy = "AuditLog_View")]
    public async Task<IActionResult> GetFilterOptions() =>
        Success(await auditLogQuery.GetFilterOptionsAsync());

    [HttpGet("{id:long}")]
    [Authorize(Policy = "AuditLog_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var log = await auditLogQuery.GetByIdAsync(id);
        return log is null ? Failure("Audit log entry not found.", StatusCodes.Status404NotFound) : Success(log);
    }

    [HttpGet("{id:long}/adjacent")]
    [Authorize(Policy = "AuditLog_View")]
    public async Task<IActionResult> GetAdjacent(long id, string? tableName = null, long? recordId = null, long? changedByUserId = null) =>
        Success(await auditLogQuery.GetAdjacentAsync(id, tableName, recordId, changedByUserId));
}
