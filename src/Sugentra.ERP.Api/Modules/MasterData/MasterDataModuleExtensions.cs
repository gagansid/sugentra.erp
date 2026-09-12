using Sugentra.ERP.Api.Modules.MasterData.Entities;
using Sugentra.ERP.Api.Modules.MasterData.Queries;
using Sugentra.ERP.Api.Modules.MasterData.Repositories;
using Sugentra.ERP.Api.Modules.MasterData.UseCases;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.MasterData;

public static class MasterDataModuleExtensions
{
    public static IServiceCollection AddMasterDataModule(this IServiceCollection services)
    {
        services.AddScoped<GenericRepository<BusinessPartner>>();
        services.AddScoped<IBusinessPartnerAddressRepository, BusinessPartnerAddressRepository>();
        services.AddScoped<IBusinessPartnerContactRepository, BusinessPartnerContactRepository>();
        services.AddScoped<BusinessPartnerUseCase>();
        services.AddScoped<BusinessPartnerListQuery>();
        services.AddScoped<Shared.Contracts.IBusinessPartnerDirectoryService, Services.BusinessPartnerDirectoryService>();

        services.AddScoped<GenericRepository<Item>>();
        services.AddScoped(sp => new CrudUseCase<Item>(
            sp.GetRequiredService<GenericRepository<Item>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "MasterData_Items"));
        services.AddScoped<ItemListQuery>();
        services.AddScoped<Shared.Contracts.IItemDirectoryService, Services.ItemDirectoryService>();

        services.AddScoped<GenericRepository<PriceListHeader>>();
        services.AddScoped<IPriceListLineRepository, PriceListLineRepository>();
        services.AddScoped<PriceListUseCase>();
        services.AddScoped<PriceListListQuery>();

        services.AddScoped<GenericRepository<BillOfMaterial>>();
        services.AddScoped<IBillOfMaterialItemRepository, BillOfMaterialItemRepository>();
        services.AddScoped<BillOfMaterialUseCase>();
        services.AddScoped<BillOfMaterialListQuery>();

        return services;
    }
}
