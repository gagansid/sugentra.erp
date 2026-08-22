using System.Net;
using System.Net.Mail;
using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Services;

/// <summary>Sends a one-off test email using SMTP settings supplied directly (not necessarily saved yet),
/// so the Email Settings page can verify a configuration before persisting it.</summary>
public class EmailConnectionTestService
{
    public async Task<Result<bool>> TestAsync(EmailSetting settings, string toEmail, string? subject = null, string? body = null)
    {
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(string.IsNullOrWhiteSpace(settings.Username) ? "no-reply@sugentra.local" : settings.Username),
                Subject = string.IsNullOrWhiteSpace(subject) ? "Sugentra ERP — Test Email" : subject,
                Body = string.IsNullOrWhiteSpace(body)
                    ? $"This is a test email sent from the '{settings.Name}' email setting to verify the SMTP configuration."
                    : body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

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
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
