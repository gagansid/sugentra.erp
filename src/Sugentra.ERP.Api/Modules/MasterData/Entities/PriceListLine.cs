using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.MasterData.Entities;

[Table("MasterData_PriceListLines")]
public class PriceListLine : BaseAuditableEntity
{
    [Required]
    public long PriceListHeaderId { get; set; }
    [Required]
    public long ItemId { get; set; }
    [Required]
    public decimal Price { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow.Date;
}
