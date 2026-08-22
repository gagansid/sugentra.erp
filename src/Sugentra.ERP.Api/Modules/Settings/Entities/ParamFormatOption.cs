using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

// Dedicated DataType -> allowed FormatString catalog (e.g. Date -> dd/MM/yyyy, Month -> MMMM_id), used by both
// the Email Template Parameters UI dropdown and Shared.Common.ParamValueFormatter at render time. No CRUD UI is
// provided for this table by design - rows are managed directly in the database via migration seeds.
[Table("Setting_ParamFormatOptions")]
public class ParamFormatOption : BaseAuditableEntity
{
    [Required, StringLength(20)]
    public string DataType { get; set; } = string.Empty;
    [Required, StringLength(50)]
    public string FormatString { get; set; } = string.Empty;
    [StringLength(200)]
    public string? Label { get; set; }
    public int SortOrder { get; set; }
}
