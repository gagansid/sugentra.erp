namespace Sugentra.ERP.UI.Models.Settings;

public record EmailTemplate
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public string? ReplyToEmail { get; set; }
    public bool IsReplyToEnabled { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public string? AvailablePlaceholders { get; set; }
    public bool IsActive { get; set; } = true;
}
