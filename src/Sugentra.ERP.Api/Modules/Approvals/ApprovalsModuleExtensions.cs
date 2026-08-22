using Sugentra.ERP.Api.Modules.Approvals.Entities;
using Sugentra.ERP.Api.Modules.Approvals.Repositories;
using Sugentra.ERP.Api.Modules.Approvals.Services;
using Sugentra.ERP.Api.Modules.Approvals.UseCases;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Approvals;

public static class ApprovalsModuleExtensions
{
    public static IServiceCollection AddApprovalsModule(this IServiceCollection services)
    {
        services.AddScoped<GenericRepository<ApprovalFlowDefinition>>();
        services.AddScoped<GenericRepository<ApprovalFlowLevel>>();
        services.AddScoped<GenericRepository<ApprovalFlowLevelApprover>>();
        services.AddScoped<GenericRepository<ApprovalRequest>>();
        services.AddScoped<GenericRepository<ApprovalRequestLevel>>();
        services.AddScoped<GenericRepository<ApprovalRequestLevelApprover>>();
        services.AddScoped<GenericRepository<ApprovalRequestAction>>();

        services.AddScoped<GenericRepository<ApprovalRoleCategory>>();
        services.AddScoped(sp => new CrudUseCase<ApprovalRoleCategory>(
            sp.GetRequiredService<GenericRepository<ApprovalRoleCategory>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Approval_RoleCategories"));

        services.AddScoped<IApprovalFlowRepository, ApprovalFlowRepository>();
        services.AddScoped<IApprovalRequestRepository, ApprovalRequestRepository>();

        services.AddScoped<ApprovalFlowUseCase>();
        services.AddScoped<ApprovalRequestUseCase>();

        // Cross-module contract — Inventory (and future modules) call this to submit documents for approval.
        services.AddScoped<IApprovalService, ApprovalService>();
        return services;
    }
}

