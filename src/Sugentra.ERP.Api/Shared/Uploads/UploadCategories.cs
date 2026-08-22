namespace Sugentra.ERP.Api.Shared.Uploads;

/// <summary>Server-side validation rules per upload category — clients cannot override these, only pick a category.</summary>
public sealed record UploadCategoryRule(string[] AllowedExtensions, long MaxSizeBytes, bool IsPublic);

/// <summary>Central registry of allowed categories. Add a new entry here to support a new kind of upload.</summary>
public static class UploadCategories
{
    public static readonly IReadOnlyDictionary<string, UploadCategoryRule> Rules = new Dictionary<string, UploadCategoryRule>(StringComparer.OrdinalIgnoreCase)
    {
        // SVG intentionally excluded - it can embed <script>, which is a stored-XSS risk if served back inline.
        ["company-logos"] = new(new[] { ".jpg", ".jpeg", ".png", ".webp" }, 2 * 1024 * 1024, IsPublic: true),
        ["business-partner-logos"] = new(new[] { ".jpg", ".jpeg", ".png", ".webp" }, 2 * 1024 * 1024, IsPublic: true),
        ["documents"] = new(new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" }, 10 * 1024 * 1024, IsPublic: false),
        ["videos"] = new(new[] { ".mp4", ".mov" }, 100 * 1024 * 1024, IsPublic: false),
    };
}
