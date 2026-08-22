namespace Sugentra.ERP.Api.Shared.ErrorLogging;

/// <summary>Record-an-error entry point, called by both the API's own GlobalExceptionHandler and the
/// UI's error-reporting endpoint (POST api/error-logs) — a single audit trail for unhandled exceptions
/// wherever they occur, so support/devs don't have to dig through console/file logs.</summary>
public interface IErrorLogService
{
    Task LogAsync(ErrorLogEntry entry);
}

public record ErrorLogEntry(
    string Source, string? Endpoint, string? HttpMethod, int? StatusCode, string? ExceptionType,
    string? Message, string? StackTrace, string? QueryString, string? RequestParameters,
    long? UserId, string? Username, string? IpAddress);

public record UpdateErrorLogStatusRequest(string Status, string? Remarks);

public record SendErrorLogEmailRequest(string ToEmail);
