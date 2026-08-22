using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Shared.Contracts;

/// <summary>Cross-module email sending contract — other modules (e.g. Identity's password reset flow) depend on
/// this interface only, never on Modules/Settings/** directly, per the module isolation rule in AGENTS.md.</summary>
public interface IEmailService
{
    /// <summary>Renders the named template with <paramref name="request"/>'s placeholders and sends it.
    /// Never throws — SMTP/template failures are captured in the returned Result and logged to Setting_EmailHistory,
    /// so callers (e.g. password reset token generation) are never broken by an email delivery failure.</summary>
    Task<Result<bool>> SendAsync(SendEmailRequest request);
}

public record EmailAttachmentRequest(string FileName, string? ContentType, byte[] Content);

public record SendEmailRequest(
    string ToEmail,
    string TemplateCode,
    // Raw values, not pre-formatted strings - EmailService looks up each key's DataType/FormatString from
    // Setting_EmailTemplateParameters and formats via Shared.Common.ParamValueFormatter before substitution.
    IReadOnlyDictionary<string, object?>? Placeholders = null,
    string? CcEmail = null,
    string? BccEmail = null,
    string? SourceModule = null,
    long? SourceReferenceId = null,
    IReadOnlyList<EmailAttachmentRequest>? Attachments = null);
