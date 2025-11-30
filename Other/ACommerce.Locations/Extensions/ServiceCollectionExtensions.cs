using ACommerce.Locations.Abstractions.Contracts;
using ACommerce.Locations.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Locations.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// تسجيل خدمات المواقع الجغرافية
    /// </summary>
    public static IServiceCollection AddACommerceLocations(this IServiceCollection services)
    {
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IGeoService, GeoService>();

        return services;
    }
}
