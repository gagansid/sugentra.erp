using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Modules.Settings.Queries;
using Sugentra.ERP.Api.Modules.Settings.Repositories;
using Sugentra.ERP.Api.Modules.Settings.Services;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Contracts;
using Sugentra.ERP.Api.Shared.Persistence;
using Module = Sugentra.ERP.Api.Modules.Settings.Entities.Module;

namespace Sugentra.ERP.Api.Modules.Settings;

public static class SettingsModuleExtensions
{
    public static IServiceCollection AddSettingsModule(this IServiceCollection services)
    {
        services.AddScoped<GenericRepository<CompanyProfile>>();
        services.AddScoped(sp => new CrudUseCase<CompanyProfile>(
            sp.GetRequiredService<GenericRepository<CompanyProfile>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_CompanyProfile"));

        services.AddScoped<GenericRepository<Warehouse>>();
        services.AddScoped(sp => new CrudUseCase<Warehouse>(
            sp.GetRequiredService<GenericRepository<Warehouse>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_Warehouses"));

        services.AddScoped<GenericRepository<Currency>>();
        services.AddScoped(sp => new CrudUseCase<Currency>(
            sp.GetRequiredService<GenericRepository<Currency>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_Currencies"));

        services.AddScoped<GenericRepository<UnitOfMeasurement>>();
        services.AddScoped(sp => new CrudUseCase<UnitOfMeasurement>(
            sp.GetRequiredService<GenericRepository<UnitOfMeasurement>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_UnitsOfMeasurement"));

        services.AddScoped<GenericRepository<Incoterm>>();
        services.AddScoped(sp => new CrudUseCase<Incoterm>(
            sp.GetRequiredService<GenericRepository<Incoterm>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_Incoterms"));

        services.AddScoped<GenericRepository<Port>>();
        services.AddScoped(sp => new CrudUseCase<Port>(
            sp.GetRequiredService<GenericRepository<Port>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_Ports"));

        services.AddScoped<GenericRepository<ApprovalMatrix>>();
        services.AddScoped(sp => new CrudUseCase<ApprovalMatrix>(
            sp.GetRequiredService<GenericRepository<ApprovalMatrix>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_ApprovalMatrices"));

        services.AddScoped<GenericRepository<DocumentNumbering>>();
        services.AddScoped(sp => new CrudUseCase<DocumentNumbering>(
            sp.GetRequiredService<GenericRepository<DocumentNumbering>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_DocumentNumberings"));
        services.AddScoped<IDocumentNumberingRepository, DocumentNumberingRepository>();
        services.AddScoped<DocumentNumberGeneratorService>();
        services.AddScoped<Shared.Contracts.IDocumentNumberGeneratorService>(sp => sp.GetRequiredService<DocumentNumberGeneratorService>());

        services.AddScoped<GenericRepository<Module>>();
        services.AddScoped(sp => new CrudUseCase<Module>(
            sp.GetRequiredService<GenericRepository<Module>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_Modules"));

        services.AddScoped<GenericRepository<Menu>>();
        services.AddScoped(sp => new CrudUseCase<Menu>(
            sp.GetRequiredService<GenericRepository<Menu>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_Menus"));

        services.AddScoped<GenericRepository<EmailSetting>>();
        services.AddScoped(sp => new CrudUseCase<EmailSetting>(
            sp.GetRequiredService<GenericRepository<EmailSetting>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_EmailSettings"));

        services.AddScoped<GenericRepository<EmailTemplate>>();
        services.AddScoped(sp => new CrudUseCase<EmailTemplate>(
            sp.GetRequiredService<GenericRepository<EmailTemplate>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_EmailTemplates"));

        services.AddScoped<GenericRepository<EmailTemplateParameter>>();
        services.AddScoped(sp => new CrudUseCase<EmailTemplateParameter>(
            sp.GetRequiredService<GenericRepository<EmailTemplateParameter>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_EmailTemplateParameters"));
        services.AddScoped<IEmailTemplateParameterRepository, EmailTemplateParameterRepository>();

        services.AddScoped<GenericRepository<SystemParameter>>();
        services.AddScoped(sp => new CrudUseCase<SystemParameter>(
            sp.GetRequiredService<GenericRepository<SystemParameter>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_SystemParameters"));
        services.AddScoped<ISystemParameterRepository, SystemParameterRepository>();
        services.AddScoped<ISystemParameterService, SystemParameterService>();
        services.AddScoped<SystemParameterListQuery>();

        services.AddScoped<GenericRepository<ParamFormatOption>>();
        services.AddScoped(sp => new CrudUseCase<ParamFormatOption>(
            sp.GetRequiredService<GenericRepository<ParamFormatOption>>(), sp.GetRequiredService<Shared.Logging.IAuditLogService>(),
            sp.GetRequiredService<Shared.Auth.ICurrentUserService>(), "Setting_ParamFormatOptions"));

        services.AddScoped<IEmailHistoryRepository, EmailHistoryRepository>();
        services.AddScoped<EmailHistoryQuery>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<EmailConnectionTestService>();

        services.AddScoped<AuditLogQuery>();
        services.AddScoped<ModuleQuery>();
        services.AddScoped<MenuQuery>();

        return services;
    }
}
