namespace Sugentra.ERP.Api.Modules.MasterData.Dtos;

public record PriceListLineRequest(long ItemId, decimal Price, DateTime EffectiveDate);

public record CreatePriceListRequest(string Name, string Type, long CurrencyId, long? BusinessPartnerId, List<PriceListLineRequest> Lines);

// Update replaces the full line set — same simplest-correct pattern as BillOfMaterial lines.
public record UpdatePriceListRequest(string Name, string Type, long CurrencyId, long? BusinessPartnerId, bool IsActive, List<PriceListLineRequest> Lines);

public record PriceListLineResponse(long Id, long ItemId, decimal Price, DateTime EffectiveDate);

public record PriceListResponse(long Id, string Name, string Type, long CurrencyId, long? BusinessPartnerId, bool IsActive, DateTime CreatedAt, IReadOnlyList<PriceListLineResponse> Lines);
