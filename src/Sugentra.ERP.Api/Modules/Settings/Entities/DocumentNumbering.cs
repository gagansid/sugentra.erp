using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_DocumentNumberings")]
public class DocumentNumbering : BaseAuditableEntity
{
    [Required, StringLength(50)]
    public string DocumentType { get; set; } = string.Empty;
    [StringLength(20)]
    public string? Prefix { get; set; }
    [StringLength(20)]
    public string? Suffix { get; set; }
    public int NumberLength { get; set; } = 6;
    // Never | Daily | Weekly | Monthly | Yearly
    [Required, StringLength(20)]
    public string ResetPeriod { get; set; } = "Never";
    public int CurrentNumber { get; set; }
    public DateTime? LastResetDate { get; set; }
    [StringLength(100)]
    public string? FormatTemplate { get; set; }
}
