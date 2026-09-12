using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Procurement.Entities;

[Table("Procurement_PurchaseRequisitionLines")]
public class PurchaseRequisitionLine : BaseAuditableEntity
{
    [Required]
    public long RequisitionId { get; set; }
    [Required]
    public long ItemId { get; set; }
    public decimal Quantity { get; set; }
    [StringLength(200)]
    public string? Notes { get; set; }
}
