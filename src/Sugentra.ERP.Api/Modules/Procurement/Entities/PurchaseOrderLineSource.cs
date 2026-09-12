using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Procurement.Entities;

// Breakdown row: how much of a merged PurchaseOrderLine's quantity came from a given PR (line).
// Null PurchaseRequisitionId means that portion was added manually, not sourced from any PR.
[Table("Procurement_PurchaseOrderLineSources")]
public class PurchaseOrderLineSource : BaseAuditableEntity
{
    public long PurchaseOrderLineId { get; set; }
    public long? PurchaseRequisitionId { get; set; }
    public long? PurchaseRequisitionLineId { get; set; }
    public decimal Quantity { get; set; }
}
