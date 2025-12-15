using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Admin.AuditLog;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminAuditLog(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
