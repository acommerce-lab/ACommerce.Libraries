using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Admin.Reports;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminReports(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
