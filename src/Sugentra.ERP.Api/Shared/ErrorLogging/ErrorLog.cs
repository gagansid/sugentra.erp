using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Shared.ErrorLogging;

[Table("Shared_ErrorLogs")]
public class ErrorLog : BaseAuditableEntity
{
    public string Source { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public int? StatusCode { get; set; }
    public string? ExceptionType { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
    public string? QueryString { get; set; }
    public string? RequestParameters { get; set; }
    public long? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public string Status { get; set; } = "New";
}
