using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.Versions.Contracts;
using ACommerce.Versions.Entities;
using ACommerce.Versions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Versions.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// إضافة خدمات إدارة الإصدارات
    /// </summary>
    public static IServiceCollection AddVersioning(this IServiceCollection services)
    {
        EntityDiscoveryRegistry.RegisterEntity<AppVersion>();
        services.AddScoped<IAppVersionService, AppVersionService>();
        return services;
    }
}
