namespace Sugentra.ERP.Api.Modules.Settings.Dtos;

public class MenuNode
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Controller { get; set; }
    public string? Action { get; set; }
    public List<MenuNode> Children { get; set; } = [];
}

/// <summary>One sidebar group = one module, with its permission-filtered menu tree.
/// HasWorkspace mirrors Setting_Modules.Route: modules with a Route get a dedicated landing/workspace page
/// (module-scoped sidebar + "back to modules"); modules without one (Audit Log, UI Kit, System Administration)
/// render as a flat top-level group instead.</summary>
public class MenuModuleGroup
{
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? ModuleIcon { get; set; }
    public bool HasWorkspace { get; set; }
    public List<MenuNode> Items { get; set; } = [];
}
