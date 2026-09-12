using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Procurement.Entities;

[Table("Procurement_PurchaseOrderLines")]
public class PurchaseOrderLine : BaseAuditableEntity
{
    public long PurchaseOrderId { get; set; }
    public long ItemId { get; set; }
    public long WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    // Updated by Procurement when Inventory posts a Goods Receipt referencing this PO (via IPurchaseOrderReceiptService).
    public decimal ReceivedQuantity { get; set; }
}
