using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Queries;

public record EmailAttachmentDto(long Id, string FileName, string? ContentType, long FileSize, string StoragePath);

public record EmailHistoryListItemDto(
    long Id, string FromEmail, string? FromName, string ToEmail, string? CcEmail, string? BccEmail,
    string Subject, string? TemplateCode, string? SourceModule, long? SourceReferenceId,
    string Status, string? ErrorMessage, int RetryCount, int AttachmentCount, DateTime SentAt);

public record EmailHistoryDetailDto(
    long Id, string FromEmail, string? FromName, string ToEmail, string? CcEmail, string? BccEmail,
    string Subject, string BodyHtml, string? TemplateCode, string? SourceModule, long? SourceReferenceId,
    string Status, string? ErrorMessage, int RetryCount, int AttachmentCount, DateTime SentAt,
    IReadOnlyList<EmailAttachmentDto> Attachments);

public record EmailHistoryListRequest(
    string? ToEmail = null, string? TemplateCode = null, string? SourceModule = null, string? Status = null,
    DateTime? FromDate = null, DateTime? ToDate = null, int Page = 1, int PageSize = 20);

public record EmailHistoryAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);

/// <summary>Read path for Setting_EmailHistory/Setting_EmailAttachments — raw Dapper, no business rules,
/// bypasses UseCase/Repository per the Queries/ convention. History is written only by EmailService.</summary>
public class EmailHistoryQuery(IDbConnectionFactory connectionFactory)
{
    private const string PagedSql = """
        SELECT Id, FromEmail, FromName, ToEmail, CcEmail, BccEmail, Subject, TemplateCode, SourceModule, SourceReferenceId,
               Status, ErrorMessage, RetryCount, AttachmentCount, SentAt, COUNT(*) OVER() AS TotalCount
        FROM Setting_EmailHistory
        WHERE IsDeleted = 0
          AND (@ToEmail IS NULL OR ToEmail LIKE '%' + @ToEmail + '%')
          AND (@TemplateCode IS NULL OR TemplateCode = @TemplateCode)
          AND (@SourceModule IS NULL OR SourceModule = @SourceModule)
          AND (@Status IS NULL OR Status = @Status)
          AND (@FromDate IS NULL OR SentAt >= @FromDate)
          AND (@ToDate IS NULL OR SentAt < DATEADD(DAY, 1, @ToDate))
        ORDER BY SentAt DESC
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
        """;

    private const string ByIdSql = """
        SELECT Id, FromEmail, FromName, ToEmail, CcEmail, BccEmail, Subject, BodyHtml, TemplateCode, SourceModule, SourceReferenceId,
               Status, ErrorMessage, RetryCount, AttachmentCount, SentAt
        FROM Setting_EmailHistory
        WHERE IsDeleted = 0 AND Id = @Id
        """;

    private const string AttachmentsByHistoryIdSql = """
        SELECT Id, FileName, ContentType, FileSize, StoragePath
        FROM Setting_EmailAttachments
        WHERE IsDeleted = 0 AND EmailHistoryId = @EmailHistoryId
        ORDER BY Id
        """;

    private const string AdjacentSql = """
        SELECT (SELECT MAX(Id) FROM Setting_EmailHistory WHERE IsDeleted = 0 AND SentAt < c.SentAt) AS PreviousId,
               (SELECT MIN(Id) FROM Setting_EmailHistory WHERE IsDeleted = 0 AND SentAt > c.SentAt) AS NextId,
               (SELECT TOP 1 Id FROM Setting_EmailHistory WHERE IsDeleted = 0 ORDER BY SentAt ASC, Id ASC) AS FirstId,
               (SELECT TOP 1 Id FROM Setting_EmailHistory WHERE IsDeleted = 0 ORDER BY SentAt DESC, Id DESC) AS LastId
        FROM (SELECT SentAt FROM Setting_EmailHistory WHERE Id = @Id) c
        """;

    public async Task<PagedResult<EmailHistoryListItemDto>> GetPagedAsync(EmailHistoryListRequest request)
    {
        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryListAsync<EmailHistoryRow>(PagedSql, request);

        return new PagedResult<EmailHistoryListItemDto>
        {
            Items = rows.Select(ToListDto).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = rows.Count > 0 ? rows[0].TotalCount : 0
        };
    }

    public async Task<EmailHistoryDetailDto?> GetByIdAsync(long id)
    {
        using var connection = connectionFactory.CreateConnection();
        var row = await connection.QuerySingleAsync<EmailHistoryDetailRow?>(ByIdSql, new { Id = id });
        if (row is null)
        {
            return null;
        }

        var attachments = await connection.QueryListAsync<EmailAttachmentDto>(AttachmentsByHistoryIdSql, new { EmailHistoryId = id });

        return new EmailHistoryDetailDto(
            row.Id, row.FromEmail, row.FromName, row.ToEmail, row.CcEmail, row.BccEmail,
            row.Subject, row.BodyHtml, row.TemplateCode, row.SourceModule, row.SourceReferenceId,
            row.Status, row.ErrorMessage, row.RetryCount, row.AttachmentCount, row.SentAt, attachments);
    }

    public async Task<EmailHistoryAdjacentDto> GetAdjacentAsync(long id)
    {
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<EmailHistoryAdjacentDto>(AdjacentSql, new { Id = id });
        return result ?? new EmailHistoryAdjacentDto(null, null, null, null);
    }

    private static EmailHistoryListItemDto ToListDto(EmailHistoryRow r) =>
        new(r.Id, r.FromEmail, r.FromName, r.ToEmail, r.CcEmail, r.BccEmail, r.Subject, r.TemplateCode, r.SourceModule,
            r.SourceReferenceId, r.Status, r.ErrorMessage, r.RetryCount, r.AttachmentCount, r.SentAt);

    private record EmailHistoryRow(
        long Id, string FromEmail, string? FromName, string ToEmail, string? CcEmail, string? BccEmail,
        string Subject, string? TemplateCode, string? SourceModule, long? SourceReferenceId,
        string Status, string? ErrorMessage, int RetryCount, int AttachmentCount, DateTime SentAt, int TotalCount = 0);

    private record EmailHistoryDetailRow(
        long Id, string FromEmail, string? FromName, string ToEmail, string? CcEmail, string? BccEmail,
        string Subject, string BodyHtml, string? TemplateCode, string? SourceModule, long? SourceReferenceId,
        string Status, string? ErrorMessage, int RetryCount, int AttachmentCount, DateTime SentAt);
}
