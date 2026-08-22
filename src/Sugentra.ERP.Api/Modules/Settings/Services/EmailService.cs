using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Modules.Settings.Queries;
using Sugentra.ERP.Api.Modules.Settings.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Services;

/// <summary>Owning implementation of Shared.Contracts.IEmailService — resolves the active Setting_EmailSettings
/// row + the requested Setting_EmailTemplates row, substitutes {{Placeholder}} tokens, sends via SMTP, and always
/// logs the attempt to Setting_EmailHistory regardless of outcome.</summary>
public class EmailService(
    GenericRepository<EmailSetting> settingsRepository,
    GenericRepository<EmailTemplate> templateRepository,
    IEmailTemplateParameterRepository parameterRepository,
    IEmailHistoryRepository historyRepository,
    ICurrentUserService currentUserService) : IEmailService
{
    private static readonly Regex PlaceholderPattern = new("{{(.*?)}}", RegexOptions.Compiled);

    public async Task<Result<bool>> SendAsync(SendEmailRequest request)
    {
        var settings = (await settingsRepository.GetAllAsync()).FirstOrDefault(s => s.IsActive);
        var template = (await templateRepository.GetAllAsync())
            .FirstOrDefault(t => t.IsActive && t.Code == request.TemplateCode);

        if (settings is null || template is null)
        {
            var error = settings is null ? "No active email settings configured." : $"Email template '{request.TemplateCode}' not found or inactive.";
            await LogHistoryAsync(request, template, error: error, status: "Failed");
            return Result<bool>.Failure(error);
        }

        var parameterMeta = (await parameterRepository.GetByTemplateCodeAsync(request.TemplateCode))
            .ToDictionary(p => p.ParamKey, p => p, StringComparer.OrdinalIgnoreCase);
        var placeholders = request.Placeholders ?? new Dictionary<string, object?>();
        var subject = Render(template.Subject, placeholders, parameterMeta);
        var bodyHtml = Render(template.BodyHtml, placeholders, parameterMeta);

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(template.FromEmail, template.FromName),
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            };
            message.To.Add(request.ToEmail);
            if (!string.IsNullOrWhiteSpace(request.CcEmail)) message.CC.Add(request.CcEmail);
            if (!string.IsNullOrWhiteSpace(request.BccEmail)) message.Bcc.Add(request.BccEmail);
            if (template.IsReplyToEnabled && !string.IsNullOrWhiteSpace(template.ReplyToEmail))
            {
                message.ReplyToList.Add(new MailAddress(template.ReplyToEmail));
            }

            foreach (var attachment in request.Attachments ?? Array.Empty<EmailAttachmentRequest>())
            {
                message.Attachments.Add(new Attachment(new MemoryStream(attachment.Content), attachment.FileName, attachment.ContentType));
            }

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.ConnectionSecurity != "None",
                Timeout = settings.TimeoutSeconds * 1000
            };
            if (!string.IsNullOrWhiteSpace(settings.Username))
            {
                client.Credentials = new NetworkCredential(settings.Username, settings.Password);
            }

            await client.SendMailAsync(message);

            await LogHistoryAsync(request, template, error: null, status: "Sent", subject: subject, bodyHtml: bodyHtml);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await LogHistoryAsync(request, template, error: ex.Message, status: "Failed", subject: subject, bodyHtml: bodyHtml);
            return Result<bool>.Failure(ex.Message);
        }
    }

    private static string Render(string text, IReadOnlyDictionary<string, object?> placeholders, IReadOnlyDictionary<string, EmailTemplateParameterRow> parameterMeta) =>
        PlaceholderPattern.Replace(text, m =>
        {
            var key = m.Groups[1].Value;
            if (!placeholders.TryGetValue(key, out var value)) return m.Value;
            parameterMeta.TryGetValue(key, out var meta);
            return ParamValueFormatter.Format(meta?.DataType, meta?.FormatString, value);
        });

    private async Task LogHistoryAsync(SendEmailRequest request, EmailTemplate? template, string? error, string status, string? subject = null, string? bodyHtml = null)
    {
        var history = new EmailHistory
        {
            FromEmail = template?.FromEmail ?? string.Empty,
            FromName = template?.FromName,
            ToEmail = request.ToEmail,
            CcEmail = request.CcEmail,
            BccEmail = request.BccEmail,
            Subject = subject ?? template?.Subject ?? string.Empty,
            BodyHtml = bodyHtml ?? template?.BodyHtml ?? string.Empty,
            TemplateCode = request.TemplateCode,
            SourceModule = request.SourceModule,
            SourceReferenceId = request.SourceReferenceId,
            Status = status,
            ErrorMessage = error,
            AttachmentCount = request.Attachments?.Count ?? 0,
            SentAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserId
        };

        var historyId = await historyRepository.AddHistoryAsync(history);

        if (request.Attachments is { Count: > 0 })
        {
            var attachments = request.Attachments
                .Select(a => new EmailAttachment
                {
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    FileSize = a.Content.LongLength,
                    StoragePath = string.Empty,
                    CreatedBy = currentUserService.UserId
                })
                .ToList();

            await historyRepository.AddAttachmentsAsync(historyId, attachments);
        }
    }
}
