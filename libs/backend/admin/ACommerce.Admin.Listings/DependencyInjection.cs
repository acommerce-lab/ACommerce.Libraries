using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Admin.Listings;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminListings(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
