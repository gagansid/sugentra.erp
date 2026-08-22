namespace Sugentra.ERP.Api.Modules.Inventory.Dtos;

public record StockOpnameLineRequest(long ItemId, long? BatchId, decimal SystemQuantity, decimal CountedQuantity, string? Notes);

public record CreateStockOpnameRequest(long WarehouseId, DateTime OpnameDate, string? Notes, List<StockOpnameLineRequest> Lines);

// Update replaces the full line set — simplest correct behavior, avoids incremental add/remove diffing.
public record UpdateStockOpnameRequest(DateTime OpnameDate, string? Notes, List<StockOpnameLineRequest> Lines);

public record StockOpnameLineResponse(long Id, long ItemId, long? BatchId, decimal SystemQuantity, decimal CountedQuantity, decimal VarianceQuantity, string? Notes);

public record StockOpnameResponse(
    long Id, string OpnameNumber, long WarehouseId, DateTime OpnameDate, string Status, string? Notes, DateTime CreatedAt,
    IReadOnlyList<StockOpnameLineResponse> Lines);
