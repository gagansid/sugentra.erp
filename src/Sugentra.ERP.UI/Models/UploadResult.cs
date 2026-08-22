namespace Sugentra.ERP.UI.Models;

/// <summary>Mirrors Sugentra.ERP.Api.Shared.Uploads.UploadFile - only the fields the UI needs after an upload.</summary>
public record UploadResult
{
    public long Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
