namespace Sugentra.ERP.Api.Modules.MasterData.Dtos;

public record BillOfMaterialLineRequest(long ComponentItemId, decimal Quantity, long UnitOfMeasurementId, string? Notes);

public record CreateBillOfMaterialRequest(long ItemId, string Name, string? Description, List<BillOfMaterialLineRequest> Lines);

// Update replaces the full line set — simplest correct behavior for Phase 1, avoids incremental add/remove diffing.
public record UpdateBillOfMaterialRequest(string Name, string? Description, bool IsActive, List<BillOfMaterialLineRequest> Lines);

public record BillOfMaterialLineResponse(long Id, long ComponentItemId, decimal Quantity, long UnitOfMeasurementId, string? Notes);

public record BillOfMaterialResponse(long Id, long ItemId, string Name, string? Description, bool IsActive, DateTime CreatedAt, IReadOnlyList<BillOfMaterialLineResponse> Lines);
