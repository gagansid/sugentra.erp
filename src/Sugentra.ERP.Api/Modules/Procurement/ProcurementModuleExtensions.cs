using Sugentra.ERP.Api.Modules.Procurement.Entities;
using Sugentra.ERP.Api.Modules.Procurement.Queries;
using Sugentra.ERP.Api.Modules.Procurement.Repositories;
using Sugentra.ERP.Api.Modules.Procurement.Services;
using Sugentra.ERP.Api.Modules.Procurement.UseCases;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Procurement;

public static class ProcurementModuleExtensions
{
    public static IServiceCollection AddProcurementModule(this IServiceCollection services)
    {
        services.AddScoped<GenericRepository<PurchaseRequisition>>();
        services.AddScoped<IPurchaseRequisitionLineRepository, PurchaseRequisitionLineRepository>();
        services.AddScoped<PurchaseRequisitionUseCase>();
        services.AddScoped<IApprovalDocumentHandler, PurchaseRequisitionApprovalHandler>();

        services.AddScoped<GenericRepository<PurchaseOrder>>();
        services.AddScoped<IPurchaseOrderLineRepository, PurchaseOrderLineRepository>();
        services.AddScoped<IPurchaseOrderRequisitionRepository, PurchaseOrderRequisitionRepository>();
        services.AddScoped<IPurchaseOrderLineSourceRepository, PurchaseOrderLineSourceRepository>();
        services.AddScoped<PurchaseOrderUseCase>();
        services.AddScoped<IPurchaseOrderReceiptService>(sp => sp.GetRequiredService<PurchaseOrderUseCase>());
        services.AddScoped<IApprovalDocumentHandler, PurchaseOrderApprovalHandler>();

        services.AddScoped<ProcurementAdjacentQuery>();

        return services;
    }
}
