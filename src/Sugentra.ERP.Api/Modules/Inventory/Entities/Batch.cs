using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.Inventory.Entities;

[Table("Inventory_Batches")]
public class Batch : BaseAuditableEntity
{
    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;
    [Required]
    public long ItemId { get; set; }
    [StringLength(50)]
    public string? Grade { get; set; }
    [Required]
    public long WarehouseId { get; set; }
    [Required]
    public DateTime ReceivedDate { get; set; }
    [StringLength(30)]
    public string? LegalityDocumentType { get; set; } // SVLK | FSC | Other
    [StringLength(100)]
    public string? LegalityDocumentNumber { get; set; }
    [StringLength(500)]
    public string? LegalityDocumentUrl { get; set; }
    [StringLength(100)]
    public string? SourceReference { get; set; }
}
