using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Procurement.Entities;

// Junction row: a PO can be sourced from multiple PRs, and a PR may be split across multiple POs.
[Table("Procurement_PurchaseOrderRequisitions")]
public class PurchaseOrderRequisition : BaseAuditableEntity
{
    public long PurchaseOrderId { get; set; }
    public long PurchaseRequisitionId { get; set; }
}
