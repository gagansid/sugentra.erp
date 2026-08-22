using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Modules.Inventory.Queries;
using Sugentra.ERP.Api.Modules.Inventory.Repositories;
using Sugentra.ERP.Api.Modules.Inventory.Services;
using Sugentra.ERP.Api.Modules.Inventory.UseCases;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory;

public static class InventoryModuleExtensions
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddScoped<GenericRepository<Batch>>();
        services.AddScoped(sp => new CrudUseCase<Batch>(
            sp.GetRequiredService<GenericRepository<Batch>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Inventory_Batches"));
        services.AddScoped<BatchUseCase>();

        services.AddScoped<GenericRepository<LandedCostAllocation>>();
        services.AddScoped(sp => new CrudUseCase<LandedCostAllocation>(
            sp.GetRequiredService<GenericRepository<LandedCostAllocation>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Inventory_LandedCostAllocations"));

        services.AddScoped<GenericRepository<QuarantineHold>>();
        services.AddScoped(sp => new CrudUseCase<QuarantineHold>(
            sp.GetRequiredService<GenericRepository<QuarantineHold>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Inventory_QuarantineHolds"));

        services.AddScoped<GenericRepository<StockBalance>>();
        services.AddScoped(sp => new CrudUseCase<StockBalance>(
            sp.GetRequiredService<GenericRepository<StockBalance>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Inventory_StockBalances"));

        services.AddScoped<GenericRepository<StockLedger>>();
        services.AddScoped(sp => new CrudUseCase<StockLedger>(
            sp.GetRequiredService<GenericRepository<StockLedger>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Inventory_StockLedgers"));

        services.AddScoped<IStockBalanceRepository, StockBalanceRepository>();

        services.AddScoped<GenericRepository<GoodsReceipt>>();
        services.AddScoped<IGoodsReceiptLineRepository, GoodsReceiptLineRepository>();
        services.AddScoped<GoodsReceiptUseCase>();
        services.AddScoped<GoodsReceiptListQuery>();
        services.AddScoped<IApprovalDocumentHandler, GoodsReceiptApprovalHandler>();

        services.AddScoped<GenericRepository<StockMutation>>();
        services.AddScoped<IStockMutationLineRepository, StockMutationLineRepository>();
        services.AddScoped<StockMutationUseCase>();

        services.AddScoped<GenericRepository<StockOpname>>();
        services.AddScoped<IStockOpnameLineRepository, StockOpnameLineRepository>();
        services.AddScoped<StockOpnameUseCase>();

        return services;
    }
}
