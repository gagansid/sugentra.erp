namespace Sugentra.ERP.Api.Shared.Contracts;

/// <summary>Cross-module contract for looking up Item master-data fields needed by other modules
/// (e.g. Inventory needs an Item's default UnitOfMeasurementId when first receiving stock for it).
/// Implemented by MasterData — callers never reference MasterData's Item entity/repository directly.</summary>
public record ItemSummary(long Id, string Code, string Name);

public interface IItemDirectoryService
{
    Task<long?> GetUnitOfMeasurementIdAsync(long itemId);

    Task<int> GetActiveCountAsync();

    Task<ItemSummary?> GetSummaryAsync(long itemId);
}
