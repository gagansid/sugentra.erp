using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Services;

public class WarehouseDirectoryService(GenericRepository<Warehouse> warehouseRepository) : IWarehouseDirectoryService
{
    public async Task<string?> GetWarehouseTypeAsync(long warehouseId)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(warehouseId);
        return warehouse?.WarehouseType;
    }
}
