namespace Sugentra.ERP.Api.Shared.Contracts;

/// <summary>Cross-module contract for looking up Warehouse master-data fields needed by other modules
/// (e.g. Inventory needs a Warehouse's type to validate Stock Mutation flows). Implemented by Settings —
/// callers never reference Settings' Warehouse entity/repository directly.</summary>
public interface IWarehouseDirectoryService
{
    Task<string?> GetWarehouseTypeAsync(long warehouseId);
}
