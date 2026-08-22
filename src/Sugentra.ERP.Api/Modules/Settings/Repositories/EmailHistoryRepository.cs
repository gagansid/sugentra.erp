using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Repositories;

/// <summary>Insert-only repository for the append-only Setting_EmailHistory/Setting_EmailAttachments log —
/// used exclusively by EmailService; there is no update/delete path for send history.</summary>
public interface IEmailHistoryRepository
{
    Task<long> AddHistoryAsync(EmailHistory history);
    Task AddAttachmentsAsync(long emailHistoryId, IReadOnlyList<EmailAttachment> attachments);
}

public class EmailHistoryRepository(IDbConnectionFactory connectionFactory) : IEmailHistoryRepository
{
    private const string InsertHistorySql = """
        INSERT INTO Setting_EmailHistory
            (FromEmail, FromName, ToEmail, CcEmail, BccEmail, Subject, BodyHtml, TemplateCode, SourceModule,
             SourceReferenceId, Status, ErrorMessage, RetryCount, AttachmentCount, SentAt, CreatedAt, CreatedBy)
        OUTPUT INSERTED.Id
        VALUES
            (@FromEmail, @FromName, @ToEmail, @CcEmail, @BccEmail, @Subject, @BodyHtml, @TemplateCode, @SourceModule,
             @SourceReferenceId, @Status, @ErrorMessage, @RetryCount, @AttachmentCount, @SentAt, GETDATE(), @CreatedBy)
        """;

    private const string InsertAttachmentSql = """
        INSERT INTO Setting_EmailAttachments (EmailHistoryId, FileName, ContentType, FileSize, StoragePath, CreatedAt, CreatedBy)
        VALUES (@EmailHistoryId, @FileName, @ContentType, @FileSize, @StoragePath, GETDATE(), @CreatedBy)
        """;

    public async Task<long> AddHistoryAsync(EmailHistory history)
    {
        using var connection = connectionFactory.CreateConnection();
        var id = await connection.QueryScalarAsync<long>(InsertHistorySql, history);
        history.Id = id;
        return id;
    }

    public async Task AddAttachmentsAsync(long emailHistoryId, IReadOnlyList<EmailAttachment> attachments)
    {
        if (attachments.Count == 0)
        {
            return;
        }

        using var connection = connectionFactory.CreateConnection();
        foreach (var attachment in attachments)
        {
            attachment.EmailHistoryId = emailHistoryId;
            await connection.ExecuteCommandAsync(InsertAttachmentSql, attachment);
        }
    }
}
