using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_EmailSettings")]
public class EmailSetting : BaseAuditableEntity
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;
    [Required, StringLength(50)]
    public string Provider { get; set; } = "Smtp";
    [Required, StringLength(200)]
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    // None | SslOnConnect | StartTls | StartTlsWhenAvailable
    [Required, StringLength(30)]
    public string ConnectionSecurity { get; set; } = "StartTls";
    public int TimeoutSeconds { get; set; } = 30;
    [StringLength(256)]
    public string? Username { get; set; }
    [StringLength(500)]
    public string? Password { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastTestedAt { get; set; }
    [StringLength(20)]
    public string? LastTestStatus { get; set; }
    [StringLength(1000)]
    public string? LastTestMessage { get; set; }
}
