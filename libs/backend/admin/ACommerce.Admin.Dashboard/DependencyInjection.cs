using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Admin.Dashboard;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminDashboard(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
