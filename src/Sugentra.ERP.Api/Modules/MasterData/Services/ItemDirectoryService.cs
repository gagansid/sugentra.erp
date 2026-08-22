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
}
