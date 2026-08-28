namespace Sugentra.ERP.UI.Models.Settings;

public record Holiday
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
}
