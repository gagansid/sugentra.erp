using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Settings.Entities;

[Table("Setting_ApprovalMatrices")]
public class ApprovalMatrix : BaseAuditableEntity
{
    [Required, StringLength(50)]
    public string DocumentType { get; set; } = string.Empty;
    public decimal MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public long? CurrencyId { get; set; }
    public int ApprovalLevel { get; set; }
    public long ApproverRoleId { get; set; }
}
