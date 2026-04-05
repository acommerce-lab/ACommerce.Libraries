using Microsoft.Extensions.DependencyInjection;
using ACommerce.Locations.Dev;
using ACommerce.Locations.Dev.Services;
using ACommerce.Locations.Abstractions.Contracts;

namespace ACommerce.Locations.Extensions;

public static class DevServiceCollectionExtensions
{
    /// <summary>
    /// Register the in-memory locations implementation for development/demo.
    /// This replaces the EF-backed implementation with a lightweight in-memory provider and a small geo helper.
    /// </summary>
    public static IServiceCollection AddACommerceLocationsInMemory(this IServiceCollection services)
    {
        // single InMemory store instance
        services.AddSingleton<InMemoryLocationService>();

        // Dev adapters that implement the Abstractions contracts
        services.AddSingleton<ILocationService, DevLocationService>();
        services.AddSingleton<IGeoService, DevGeoService>();

        return services;
    }
}
