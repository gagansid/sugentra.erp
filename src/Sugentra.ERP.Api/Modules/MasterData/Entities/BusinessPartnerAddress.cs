using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.MasterData.Entities;

[Table("MasterData_BusinessPartnerAddresses")]
public class BusinessPartnerAddress : BaseAuditableEntity
{
    [Required]
    public long BusinessPartnerId { get; set; }
    [Required, StringLength(20)]
    public string AddressType { get; set; } = string.Empty; // Office | Billing | Shipping | Other
    [Required, StringLength(500)]
    public string Address { get; set; } = string.Empty;
    [StringLength(100)]
    public string? City { get; set; }
    [StringLength(100)]
    public string? Province { get; set; }
    [StringLength(20)]
    public string? PostalCode { get; set; }
    [StringLength(100)]
    public string? Country { get; set; }
    public bool IsPrimary { get; set; }
}
