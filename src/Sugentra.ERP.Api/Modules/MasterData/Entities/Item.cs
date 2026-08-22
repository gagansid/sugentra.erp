using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.MasterData.Entities;

[Table("MasterData_Items")]
public class Item : BaseAuditableEntity
{
    [Required, StringLength(30)]
    public string Code { get; set; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [Required, StringLength(30)]
    public string Category { get; set; } = string.Empty; // RawMaterial | SemiFinished | FinishedGood
    [StringLength(50)]
    public string? Grade { get; set; }
    [Required]
    public long UnitOfMeasurementId { get; set; }
    public decimal StandardPrice { get; set; }
    public bool IsActive { get; set; } = true;
}
