namespace Sugentra.ERP.UI.Models.Settings;

// Branding-only projection of CompanyProfile (name + logo), returned by GET api/settings/company-profile/identity.
public record CompanyIdentity(string CompanyName, string? LogoUrl);
