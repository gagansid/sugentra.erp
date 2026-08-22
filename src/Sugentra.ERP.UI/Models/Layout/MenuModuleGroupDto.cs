namespace Sugentra.ERP.UI.Models.Layout;

public class MenuNodeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Controller { get; set; }
    public string? Action { get; set; }
    public List<MenuNodeDto> Children { get; set; } = [];
}

public class MenuModuleGroupDto
{
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? ModuleIcon { get; set; }
    public bool HasWorkspace { get; set; }
    public List<MenuNodeDto> Items { get; set; } = [];
}
