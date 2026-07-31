using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Shared.Logging;

[Table("Setting_AuditLogs")]
public class AuditLog : BaseAuditableEntity
{
    public string TableName { get; set; } = string.Empty;
    public long RecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public long ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}
