using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.MasterData.Entities;

[Table("MasterData_PriceListHeaders")]
public class PriceListHeader : BaseAuditableEntity
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    // "Sales" or "Purchase".
    [Required, StringLength(20)]
    public string Type { get; set; } = "Sales";
    [Required]
    public long CurrencyId { get; set; }
    // Null = general list for this Type + Currency; set = customer/vendor-specific list.
    public long? BusinessPartnerId { get; set; }
    public bool IsActive { get; set; } = true;
}
