using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_EmailAttachments")]
public class EmailAttachment : BaseAuditableEntity
{
    public long EmailHistoryId { get; set; }
    [Required, StringLength(300)]
    public string FileName { get; set; } = string.Empty;
    [StringLength(150)]
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    // Path/URL to the stored file, not a VARBINARY blob.
    [Required, StringLength(1000)]
    public string StoragePath { get; set; } = string.Empty;
}
