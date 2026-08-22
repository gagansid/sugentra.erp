using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.ErrorLogging;

namespace Sugentra.ERP.Api.Controllers;

// Generic cross-cutting error log endpoint - written by the API's own GlobalExceptionHandler and by the UI's
// /Error page (so unhandled MVC exceptions land in the same audit trail as API exceptions).
[Route("api/error-logs")]
public class ErrorLogsController(
    IErrorLogService errorLogService,
    ErrorLogQuery errorLogQuery,
    ICurrentUserService currentUserService,
    IEmailService emailService,
    ISystemParameterService systemParameterService,
    IUserDirectoryService userDirectoryService) : ApiControllerBase
{
    // AllowAnonymous: an unhandled exception can happen before/without authentication (e.g. login page itself
    // erroring), and this call is only ever made server-to-server from the UI's own exception handler, never
    // from browser JS.
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Report([FromBody] ErrorLogEntry entry)
    {
        await errorLogService.LogAsync(entry);
        return SuccessMessage("Error logged.");
    }

    [HttpGet]
    [Authorize(Policy = "ErrorLog_View")]
    public async Task<IActionResult> GetPaged([FromQuery] ErrorLogListRequest request) =>
        Success(await errorLogQuery.GetPagedAsync(request));

    [HttpGet("{id:long}")]
    [Authorize(Policy = "ErrorLog_View")]
    public async Task<IActionResult> GetById(long id)
    {
        var entry = await errorLogQuery.GetByIdAsync(id);
        return entry is null ? Failure("Error log entry not found.", StatusCodes.Status404NotFound) : Success(entry);
    }

    [HttpGet("{id:long}/adjacent")]
    [Authorize(Policy = "ErrorLog_View")]
    public async Task<IActionResult> GetAdjacent(long id) =>
        Success(await errorLogQuery.GetAdjacentAsync(id));

    [HttpGet("default-notification-email")]
    [Authorize(Policy = "ErrorLog_SendEmail")]
    public async Task<IActionResult> GetDefaultNotificationEmail() =>
        Success(await systemParameterService.GetStringAsync("ErrorLog", "NotificationEmail"));

    // Only 2 conditions - NotSolved (default) and Solved. Sending a notification email is a separate action,
    // not a condition value.
    private static readonly string[] AllowedStatuses = ["NotSolved", "Solved"];

    [HttpPut("{id:long}/status")]
    [Authorize(Policy = "ErrorLog_Edit")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateErrorLogStatusRequest request)
    {
        if (!AllowedStatuses.Contains(request.Status))
        {
            return Failure("Invalid condition.", StatusCodes.Status422UnprocessableEntity);
        }

        var updated = await errorLogQuery.UpdateStatusAsync(id, request.Status, request.Remarks, currentUserService.UserId);
        return updated ? SuccessMessage("Condition updated.") : Failure("Error log entry not found.", StatusCodes.Status404NotFound);
    }

    [HttpPost("{id:long}/send-email")]
    [Authorize(Policy = "ErrorLog_SendEmail")]
    public async Task<IActionResult> SendEmail(long id, [FromBody] SendErrorLogEmailRequest request)
    {
        var entry = await errorLogQuery.GetByIdAsync(id);
        if (entry is null)
        {
            return Failure("Error log entry not found.", StatusCodes.Status404NotFound);
        }

        // Cc the sender (so they keep a copy) and the original reporter who ran into the error.
        var sender = currentUserService.UserId is long senderId ? await userDirectoryService.GetByIdAsync(senderId) : null;
        var ccEmails = new[] { sender?.Email, entry.ReporterEmail }
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var statusLabel = entry.Status == "Solved" ? "Solved" : "Not Solved";
        var statusBadgeStyle = entry.Status == "Solved"
            ? "background-color:#d3f9d8;color:#2b8a3e;padding:2px 10px;border-radius:4px;font-size:12px;font-weight:bold;"
            : "background-color:#e9ecef;color:#495057;padding:2px 10px;border-radius:4px;font-size:12px;font-weight:bold;";

        var result = await emailService.SendAsync(new SendEmailRequest(
            ToEmail: request.ToEmail,
            TemplateCode: "ERROR_LOG_NOTIFICATION",
            Placeholders: new Dictionary<string, object?>
            {
                ["EntryId"] = entry.Id,
                ["Source"] = entry.Source,
                ["OccurredAt"] = entry.OccurredAt,
                ["Endpoint"] = entry.Endpoint,
                ["ExceptionType"] = entry.ExceptionType,
                ["Status"] = statusLabel,
                ["StatusBadge"] = $"<span style=\"{statusBadgeStyle}\">{statusLabel}</span>",
                ["Remarks"] = string.IsNullOrWhiteSpace(entry.Remarks) ? "No remarks provided." : entry.Remarks,
                ["Message"] = entry.Message
            },
            CcEmail: ccEmails.Length > 0 ? string.Join(",", ccEmails) : null,
            SourceModule: "ErrorLog",
            SourceReferenceId: entry.Id));

        if (result.IsSuccess)
        {
            var sentAt = DateTime.UtcNow;
            await errorLogQuery.UpdateEmailSentAtAsync(id, sentAt, currentUserService.UserId);

            // Solved date/by is tracked separately from EmailSentAt/By - it only applies when the
            // notification is sent while the entry is already marked Solved by IT.
            if (entry.Status == "Solved")
            {
                await errorLogQuery.UpdateSolvedAsync(id, sentAt, currentUserService.UserId);
            }
        }

        return result.IsSuccess ? SuccessMessage("Email sent.") : Failure(result.Error ?? "Failed to send email.");
    }
}
