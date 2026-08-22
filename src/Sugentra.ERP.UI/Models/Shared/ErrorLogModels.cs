namespace Sugentra.ERP.UI.Models.Shared;

public record ErrorLogEntry(
    string Source, string? Endpoint, string? HttpMethod, int? StatusCode, string? ExceptionType,
    string? Message, string? StackTrace, string? QueryString, string? RequestParameters,
    long? UserId, string? Username, string? IpAddress);

public record ErrorLogListItemDto(
    long Id, string Source, DateTime OccurredAt, string? Endpoint, string? HttpMethod, int? StatusCode,
    string? ExceptionType, string Status, string? Username);

public record ErrorLogDetailDto(
    long Id, string Source, DateTime OccurredAt, string? Endpoint, string? HttpMethod, int? StatusCode,
    string? ExceptionType, string? Message, string? StackTrace, string? QueryString, string? RequestParameters,
    string? Username, string? IpAddress, string Status, string? Remarks, DateTime? EmailSentAt,
    string? EmailSentByEmail, long? UpdatedBy, string? ReporterFullName, DateTime? SolvedAt, string? SolvedByEmail,
    string? ReporterEmail);

public record ErrorLogListRequest(
    string? Source = null, string? Search = null, DateTime? FromDate = null, DateTime? ToDate = null,
    int Page = 1, int PageSize = 10);

public record UpdateErrorLogStatusRequest(string Status, string? Remarks);

public record SendErrorLogEmailRequest(string ToEmail);

public record ErrorLogAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);
