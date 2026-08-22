namespace Sugentra.ERP.UI.Models.Settings;

public record DocumentNumbering
{
    public long Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public int NumberLength { get; set; } = 6;
    // Never | Daily | Weekly | Monthly | Yearly
    public string ResetPeriod { get; set; } = "Never";
    public int CurrentNumber { get; set; }
    public DateTime? LastResetDate { get; set; }
    public string? FormatTemplate { get; set; }
}
