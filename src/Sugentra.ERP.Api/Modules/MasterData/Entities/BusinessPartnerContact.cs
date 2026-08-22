using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.MasterData.Entities;

[Table("MasterData_BusinessPartnerContacts")]
public class BusinessPartnerContact : BaseAuditableEntity
{
    [Required]
    public long BusinessPartnerId { get; set; }
    [Required, StringLength(20)]
    public string ContactType { get; set; } = string.Empty; // Phone | WhatsApp | Fax | Email
    [StringLength(200)]
    public string? ContactName { get; set; }
    [Required, StringLength(200)]
    public string Value { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
