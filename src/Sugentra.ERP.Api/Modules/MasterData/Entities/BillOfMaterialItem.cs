using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.MasterData.Entities;

[Table("MasterData_BillOfMaterialItems")]
public class BillOfMaterialItem : BaseAuditableEntity
{
    [Required]
    public long BillOfMaterialId { get; set; }
    [Required]
    public long ComponentItemId { get; set; }
    [Required]
    public decimal Quantity { get; set; }
    [Required]
    public long UnitOfMeasurementId { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
}
