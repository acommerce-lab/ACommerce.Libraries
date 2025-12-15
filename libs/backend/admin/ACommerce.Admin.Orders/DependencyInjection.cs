using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Admin.Orders;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminOrders(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
