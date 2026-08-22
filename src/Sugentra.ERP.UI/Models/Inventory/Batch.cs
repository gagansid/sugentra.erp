namespace Sugentra.ERP.UI.Models.Inventory;

public record Batch
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public long ItemId { get; set; }
    public string? Grade { get; set; }
    public long WarehouseId { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.Today;
    public string? LegalityDocumentType { get; set; } // SVLK | FSC | Other
    public string? LegalityDocumentNumber { get; set; }
    public string? LegalityDocumentUrl { get; set; }
    public string? SourceReference { get; set; }
}
