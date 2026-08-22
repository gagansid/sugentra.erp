using System.ComponentModel.DataAnnotations;

namespace Sugentra.ERP.UI.Models.Settings;

public record CompanyProfile
{
    public long Id { get; set; }

    [Required, StringLength(200, MinimumLength = 2)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? LegalName { get; set; }
    [StringLength(50)]
    public string? TaxId { get; set; }
    [StringLength(500)]
    public string? Address { get; set; }
    [StringLength(100)]
    public string? City { get; set; }
    [StringLength(100)]
    public string? Country { get; set; }
    [Phone, StringLength(50)]
    public string? PhoneNumber { get; set; }
    [StringLength(256)]
    public string? Email { get; set; }
    [StringLength(200)]
    public string? Website { get; set; }
    [StringLength(500)]
    public string? LogoUrl { get; set; }
    [StringLength(500)]
    public string? LogoDarkUrl { get; set; }
    [StringLength(500)]
    public string? LogoSquareUrl { get; set; }
    [StringLength(500)]
    public string? IconUrl { get; set; }
    [StringLength(500)]
    public string? IconLightUrl { get; set; }
    [StringLength(500)]
    public string? IconDarkUrl { get; set; }
}

