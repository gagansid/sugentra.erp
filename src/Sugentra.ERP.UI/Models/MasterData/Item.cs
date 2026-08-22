namespace Sugentra.ERP.UI.Models.MasterData;

public record Item
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "RawMaterial"; // RawMaterial | SemiFinished | FinishedGood
    public string? Grade { get; set; }
    public long UnitOfMeasurementId { get; set; }
    public decimal StandardPrice { get; set; }
    public bool IsActive { get; set; } = true;
}

public record ItemListItemDto(long Id, string Code, string Name, string Category, string? Grade, long UnitOfMeasurementId, decimal StandardPrice, bool IsActive);

public record ItemListRequest(string? Keyword = null, int Page = 1, int PageSize = 10, bool? IsActive = null);

public record ItemAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);
