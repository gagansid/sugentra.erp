namespace Sugentra.ERP.UI.Models.Layout;

// ShowBrand/CompanyName/LogoUrl are only populated on sidebar-less pages (e.g. the dashboard),
// where the navbar itself must carry system identity since the sidebar's own brand isn't rendered.
public record NavbarViewModel(string Username, bool ShowBrand, string? CompanyName, string? LogoUrl);
