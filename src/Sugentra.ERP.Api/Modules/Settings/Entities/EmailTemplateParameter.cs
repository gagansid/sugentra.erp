using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

// Catalog of placeholders ({{ParamKey}}) available for one Setting_EmailTemplates row, replacing the free-text
// AvailablePlaceholders list with structured metadata (type/format/looping) for future dynamic rendering.
[Table("Setting_EmailTemplateParameters")]
public class EmailTemplateParameter : BaseAuditableEntity
{
    public long EmailTemplateId { get; set; }
    [Required, StringLength(100)]
    public string ParamKey { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Description { get; set; }
    // String | Int | Decimal | Float | Bool | Date | List
    [Required, StringLength(20)]
    public string DataType { get; set; } = "String";
    // True when this placeholder represents a repeating collection to loop over in the email body (e.g. table rows).
    public bool IsLoop { get; set; }
    // e.g. "N2", "0.00", "yyyy-MM-dd" - drives numeric/date formatting (including rounding) at render time.
    [StringLength(50)]
    public string? FormatString { get; set; }
    public bool IsRequired { get; set; }
    // Used when the caller doesn't supply a value at send time (e.g. ExpiresMinutes is always this default).
    [StringLength(500)]
    public string? DefaultValue { get; set; }
}
