using ACommerce.Client.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.Locations.Extensions;

/// <summary>
/// Extensions لتسجيل Locations Client
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// إضافة Locations Client
    /// </summary>
    public static IServiceCollection AddLocationsClient(
        this IServiceCollection services,
        string registryUrl)
    {
        // ACommerce Client (إذا لم يكن مسجلاً مسبقاً)
        services.AddACommerceClient(registryUrl);

        // Locations Client
        services.AddScoped<LocationsClient>();

        return services;
    }

    /// <summary>
    /// إضافة Locations Client (بدون registry - للاستخدام مع Static URL)
    /// </summary>
    public static IServiceCollection AddLocationsClient(this IServiceCollection services)
    {
        services.AddScoped<LocationsClient>();
        return services;
    }
}
