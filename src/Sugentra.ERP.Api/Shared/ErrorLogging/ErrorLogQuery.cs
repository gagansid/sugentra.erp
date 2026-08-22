using Dapper;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Shared.ErrorLogging;

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
    int Page = 1, int PageSize = 20);

public record ErrorLogAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);

/// <summary>Read path for Shared_ErrorLogs — raw Dapper, no business rules. Rows are written only by
/// ErrorLogService (GlobalExceptionHandler on the API side, or the UI's own /Error page).</summary>
public class ErrorLogQuery(IDbConnectionFactory connectionFactory)
{
    private const string PagedSql = """
        SELECT Id, Source, OccurredAt, Endpoint, HttpMethod, StatusCode, ExceptionType, Status, Username,
               COUNT(*) OVER() AS TotalCount
        FROM Shared_ErrorLogs
        WHERE IsDeleted = 0
          AND (@Source IS NULL OR Source = @Source)
          AND (@Search IS NULL OR Endpoint LIKE '%' + @Search + '%' OR Message LIKE '%' + @Search + '%' OR ExceptionType LIKE '%' + @Search + '%')
          AND (@FromDate IS NULL OR OccurredAt >= @FromDate)
          AND (@ToDate IS NULL OR OccurredAt < DATEADD(DAY, 1, @ToDate))
        ORDER BY OccurredAt DESC
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
        """;

    private const string ByIdSql = """
        SELECT e.Id, e.Source, e.OccurredAt, e.Endpoint, e.HttpMethod, e.StatusCode, e.ExceptionType, e.Message, e.StackTrace,
               e.QueryString, e.RequestParameters, e.Username, e.IpAddress, e.Status, e.Remarks, e.EmailSentAt,
               emailSender.Email AS EmailSentByEmail, e.UpdatedBy, reporter.FullName AS ReporterFullName,
               e.SolvedAt, solver.Email AS SolvedByEmail, reporter.Email AS ReporterEmail
        FROM Shared_ErrorLogs e
        LEFT JOIN Identity_Users emailSender ON emailSender.Id = e.EmailSentBy
        LEFT JOIN Identity_Users reporter ON reporter.Username = e.Username
        LEFT JOIN Identity_Users solver ON solver.Id = e.SolvedBy
        WHERE e.IsDeleted = 0 AND e.Id = @Id
        """;

    private const string UpdateEmailSentAtSql = """
        UPDATE Shared_ErrorLogs SET EmailSentAt = @EmailSentAt, EmailSentBy = @EmailSentBy WHERE IsDeleted = 0 AND Id = @Id
        """;

    private const string UpdateSolvedSql = """
        UPDATE Shared_ErrorLogs SET SolvedAt = @SolvedAt, SolvedBy = @SolvedBy WHERE IsDeleted = 0 AND Id = @Id
        """;

    private const string UpdateStatusSql = """
        UPDATE Shared_ErrorLogs SET Status = @Status, Remarks = @Remarks, UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy
        WHERE IsDeleted = 0 AND Id = @Id
        """;

    private const string AdjacentSql = """
        SELECT (SELECT MAX(Id) FROM Shared_ErrorLogs WHERE IsDeleted = 0 AND OccurredAt < c.OccurredAt) AS PreviousId,
               (SELECT MIN(Id) FROM Shared_ErrorLogs WHERE IsDeleted = 0 AND OccurredAt > c.OccurredAt) AS NextId,
               (SELECT TOP 1 Id FROM Shared_ErrorLogs WHERE IsDeleted = 0 ORDER BY OccurredAt ASC, Id ASC) AS FirstId,
               (SELECT TOP 1 Id FROM Shared_ErrorLogs WHERE IsDeleted = 0 ORDER BY OccurredAt DESC, Id DESC) AS LastId
        FROM (SELECT OccurredAt FROM Shared_ErrorLogs WHERE Id = @Id) c
        """;

    private record ErrorLogListRow(
        long Id, string Source, DateTime OccurredAt, string? Endpoint, string? HttpMethod, int? StatusCode,
        string? ExceptionType, string Status, string? Username, int TotalCount);

    public async Task<PagedResult<ErrorLogListItemDto>> GetPagedAsync(ErrorLogListRequest request)
    {
        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<ErrorLogListRow>(PagedSql, request);

        return new PagedResult<ErrorLogListItemDto>
        {
            Items = rows.Select(r => new ErrorLogListItemDto(
                r.Id, r.Source, r.OccurredAt, r.Endpoint, r.HttpMethod, r.StatusCode, r.ExceptionType, r.Status, r.Username)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }

    public async Task<ErrorLogDetailDto?> GetByIdAsync(long id)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<ErrorLogDetailDto?>(ByIdSql, new { Id = id });
    }

    public async Task<bool> UpdateStatusAsync(long id, string status, string? remarks, long? updatedBy)
    {
        using var connection = connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(UpdateStatusSql, new { Id = id, Status = status, Remarks = remarks, UpdatedBy = updatedBy });
        return rowsAffected > 0;
    }

    public async Task<ErrorLogAdjacentDto> GetAdjacentAsync(long id)
    {
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<ErrorLogAdjacentDto>(AdjacentSql, new { Id = id });
        return result ?? new ErrorLogAdjacentDto(null, null, null, null);
    }

    public async Task UpdateEmailSentAtAsync(long id, DateTime emailSentAt, long? emailSentBy)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(UpdateEmailSentAtSql, new { Id = id, EmailSentAt = emailSentAt, EmailSentBy = emailSentBy });
    }

    public async Task UpdateSolvedAsync(long id, DateTime solvedAt, long? solvedBy)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(UpdateSolvedSql, new { Id = id, SolvedAt = solvedAt, SolvedBy = solvedBy });
    }
}
