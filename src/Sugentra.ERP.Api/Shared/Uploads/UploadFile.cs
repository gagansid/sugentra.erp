using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Shared.Uploads;

[Table("Shared_Uploads")]
public class UploadFile : BaseAuditableEntity
{
    public string Category { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public long? EntityId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string StorageProvider { get; set; } = "Local";
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsPublic { get; set; }
    public string? Checksum { get; set; }
}
