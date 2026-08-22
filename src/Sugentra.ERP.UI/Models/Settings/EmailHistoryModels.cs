namespace Sugentra.ERP.UI.Models.Settings;

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
    DateTime? FromDate = null, DateTime? ToDate = null, int Page = 1, int PageSize = 10);

public record EmailHistoryAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);
