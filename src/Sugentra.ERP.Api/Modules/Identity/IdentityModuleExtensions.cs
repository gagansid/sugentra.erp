using Sugentra.ERP.Api.Modules.Identity.Queries;
using Sugentra.ERP.Api.Modules.Identity.Repositories;
using Sugentra.ERP.Api.Modules.Identity.UseCases;

namespace Sugentra.ERP.Api.Modules.Identity;

public static class IdentityModuleExtensions
{
    // Controllers here use attribute routing, so ASP.NET Core's controller discovery maps their
    // endpoints automatically via app.MapControllers() in Program.cs — no separate MapIdentityEndpoints() is needed.
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<UserUseCase>();
        services.AddScoped<UserListQuery>();
        return services;
    }
}
