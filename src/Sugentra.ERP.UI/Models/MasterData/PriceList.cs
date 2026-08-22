namespace Sugentra.ERP.UI.Models.MasterData;

public record PriceListLineRequest(long ItemId, decimal Price, DateTime EffectiveDate);

public record CreatePriceListRequest(string Name, string Type, long CurrencyId, long? BusinessPartnerId, List<PriceListLineRequest> Lines);

public record UpdatePriceListRequest(string Name, string Type, long CurrencyId, long? BusinessPartnerId, bool IsActive, List<PriceListLineRequest> Lines);

public record PriceListLineResponse(long Id, long ItemId, decimal Price, DateTime EffectiveDate);

public record PriceListResponse(long Id, string Name, string Type, long CurrencyId, long? BusinessPartnerId, bool IsActive, DateTime CreatedAt, IReadOnlyList<PriceListLineResponse> Lines);

public record PriceListListItemDto(long Id, string Name, string Type, int LineCount, bool IsActive);

public record PriceListListRequest(string? Keyword = null, int Page = 1, int PageSize = 10, bool? IsActive = null);

public record PriceListAdjacentDto(long? PreviousId, long? NextId, long? FirstId, long? LastId);
