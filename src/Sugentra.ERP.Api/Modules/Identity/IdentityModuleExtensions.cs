using Sugentra.ERP.Api.Modules.Identity.Repositories;
using Sugentra.ERP.Api.Modules.Identity.Services;
using Sugentra.ERP.Api.Modules.Identity.UseCases;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Identity;

public static class IdentityModuleExtensions
{
    // Controllers here use attribute routing, so ASP.NET Core's controller discovery maps their
    // endpoints automatically via app.MapControllers() in Program.cs — no separate MapIdentityEndpoints() is needed.
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
        services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        services.AddScoped<PermissionResolverService>();
        services.AddScoped<IUserDirectoryService, UserDirectoryService>();
        services.AddScoped<IRoleMembershipService, RoleMembershipService>();

        services.AddScoped<UserUseCase>();
        services.AddScoped<RoleUseCase>();
        services.AddScoped<PermissionUseCase>();
        services.AddScoped<AuthUseCase>();
        services.AddScoped<PasswordResetUseCase>();
        return services;
    }
}
