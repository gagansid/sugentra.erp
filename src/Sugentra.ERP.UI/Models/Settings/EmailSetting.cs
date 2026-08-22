namespace Sugentra.ERP.UI.Models.Settings;

public record EmailSetting
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = "Smtp";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string ConnectionSecurity { get; set; } = "StartTls";
    public int TimeoutSeconds { get; set; } = 30;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastTestedAt { get; set; }
    public string? LastTestStatus { get; set; }
    public string? LastTestMessage { get; set; }
}
