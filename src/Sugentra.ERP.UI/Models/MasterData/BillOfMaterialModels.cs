namespace Sugentra.ERP.UI.Models.MasterData;

public record BillOfMaterialLineRequest(long ComponentItemId, decimal Quantity, long UnitOfMeasurementId, string? Notes);

public record CreateBillOfMaterialRequest(long ItemId, string Name, string? Description, List<BillOfMaterialLineRequest> Lines);

public record UpdateBillOfMaterialRequest(string Name, string? Description, bool IsActive, List<BillOfMaterialLineRequest> Lines);

public record BillOfMaterialLineResponse(long Id, long ComponentItemId, decimal Quantity, long UnitOfMeasurementId, string? Notes);

public record BillOfMaterialResponse(long Id, long ItemId, string Name, string? Description, bool IsActive, DateTime CreatedAt, IReadOnlyList<BillOfMaterialLineResponse> Lines);

public record BillOfMaterialListItemDto(long Id, long ItemId, string Name, string? Description, int LineCount, bool IsActive);

public record BillOfMaterialListRequest(string? Keyword = null, int Page = 1, int PageSize = 10, bool? IsActive = null);

public record BillOfMaterialAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);
