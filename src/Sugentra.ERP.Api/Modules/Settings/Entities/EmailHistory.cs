using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

// Append-only send log, written only by EmailService — no CrudUseCase/UpdateAsync/DeleteAsync exposed for this entity.
[Table("Setting_EmailHistory")]
public class EmailHistory : BaseAuditableEntity
{
    [Required, StringLength(256)]
    public string FromEmail { get; set; } = string.Empty;
    [StringLength(200)]
    public string? FromName { get; set; }
    [Required, StringLength(256)]
    public string ToEmail { get; set; } = string.Empty;
    [StringLength(500)]
    public string? CcEmail { get; set; }
    [StringLength(500)]
    public string? BccEmail { get; set; }
    [Required, StringLength(300)]
    public string Subject { get; set; } = string.Empty;
    [Required]
    public string BodyHtml { get; set; } = string.Empty;
    [StringLength(100)]
    public string? TemplateCode { get; set; }
    [StringLength(100)]
    public string? SourceModule { get; set; }
    public long? SourceReferenceId { get; set; }
    [Required, StringLength(20)]
    public string Status { get; set; } = string.Empty;
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public int AttachmentCount { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
