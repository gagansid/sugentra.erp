namespace Sugentra.ERP.UI.Shared;

// Central mapping of document-lifecycle status -> badge color, so the color standard only needs updating here.
public static class StatusBadgeHelper
{
    public static string GetColor(string? status) => status switch
    {
        null => "secondary",
        "Draft" or "Pending" => "secondary",
        "WaitingApproval" or "OnHold" => "warning",
        var s when s.StartsWith("Waiting Approval - ") => "warning",
        "PartiallyReceived" => "info",
        "Approved" or "Approve" or "Completed" or "Posted" or "Post" or "FullyReceived" or "Released" => "primary",
        "Rejected" or "Reject" or "Cancelled" or "SoftDelete" => "danger",
        _ => "secondary"
    };

    public static (string Color, string Label) GetOrderStatus(string? orderStatus) => orderStatus switch
    {
        "FullyOrdered" => ("primary", "Fully Ordered"),
        "PartiallyOrdered" => ("warning", "Partially Ordered"),
        _ => ("secondary", "Not Ordered")
    };
}
