using Sugentra.ERP.Api.Modules.MasterData.Entities;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData.Services;

public class ItemDirectoryService(GenericRepository<Item> itemRepository) : IItemDirectoryService
{
    public async Task<long?> GetUnitOfMeasurementIdAsync(long itemId)
    {
        var item = await itemRepository.GetByIdAsync(itemId);
        return item?.UnitOfMeasurementId;
    }

    public async Task<int> GetActiveCountAsync() => (await itemRepository.GetAllAsync()).Count;

    public async Task<ItemSummary?> GetSummaryAsync(long itemId)
    {
        var item = await itemRepository.GetByIdAsync(itemId);
        return item is null ? null : new ItemSummary(item.Id, item.Code, item.Name);
    }
}
