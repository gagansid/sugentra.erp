using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_EmailTemplates")]
public class EmailTemplate : BaseAuditableEntity
{
    // Lookup key callers pass to IEmailService.SendAsync (e.g. "PASSWORD_RESET").
    [Required, StringLength(100)]
    public string Code { get; set; } = string.Empty;
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Description { get; set; }
    [Required, StringLength(256)]
    public string FromEmail { get; set; } = string.Empty;
    [StringLength(200)]
    public string? FromName { get; set; }
    [StringLength(256)]
    public string? ReplyToEmail { get; set; }
    public bool IsReplyToEnabled { get; set; }
    [Required, StringLength(300)]
    public string Subject { get; set; } = string.Empty;
    // Supports {{PlaceholderName}} tokens, replaced by IEmailService before sending.
    [Required]
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    // Comma-separated list of valid placeholder names, e.g. "UserName,ResetLink,ExpiryMinutes".
    [StringLength(500)]
    public string? AvailablePlaceholders { get; set; }
    public bool IsActive { get; set; } = true;
}
