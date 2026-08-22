using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Modules.MasterData.Entities;

[Table("MasterData_BusinessPartners")]
public class BusinessPartner : BaseAuditableEntity
{
    [Required, StringLength(30)]
    public string Code { get; set; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [Required, StringLength(20)]
    public string PartnerType { get; set; } = string.Empty; // Customer | Supplier | Both
    [StringLength(50)]
    public string? TaxId { get; set; } // NPWP
    [StringLength(200)]
    public string? TaxRegisteredName { get; set; }
    [StringLength(500)]
    public string? TaxAddress { get; set; }
    [StringLength(20)]
    public string? Nik { get; set; }
    [StringLength(20)]
    public string? TaxpayerType { get; set; } // Badan | OrangPribadi
    [StringLength(30)]
    public string? Nitku { get; set; }
    public bool IsPkp { get; set; }
    [StringLength(50)]
    public string? SktNumber { get; set; }
    [StringLength(20)]
    public string? KluCode { get; set; }
    [StringLength(500)]
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
