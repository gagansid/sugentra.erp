namespace Sugentra.ERP.UI.Models.Layout;

/// <summary>A single crumb; Url null means the current page (rendered as plain text, not a link).</summary>
public record BreadcrumbItem(string Text, string? Url);

public record BreadcrumbViewModel(string Icon, IReadOnlyList<BreadcrumbItem> Items);
