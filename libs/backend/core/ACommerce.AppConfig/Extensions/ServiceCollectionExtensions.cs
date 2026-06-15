using ACommerce.AppConfig.Contracts;
using ACommerce.AppConfig.Entities;
using ACommerce.AppConfig.Services;
using ACommerce.SharedKernel.Abstractions.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.AppConfig.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// تسجيل خدمة AppConfig + FeatureFlags.
    /// يسجّل الـ Entities في EntityDiscoveryRegistry ليجدها ApplicationDbContext تلقائياً.
    /// </summary>
    public static IServiceCollection AddAppConfig(this IServiceCollection services)
    {
        EntityDiscoveryRegistry.RegisterEntity<AppConfigEntry>();
        EntityDiscoveryRegistry.RegisterEntity<UiString>();
        EntityDiscoveryRegistry.RegisterEntity<ThemeToken>();
        EntityDiscoveryRegistry.RegisterEntity<FeatureFlag>();

        services.AddMemoryCache();
        services.AddScoped<IFeatureFlagsService, FeatureFlagsService>();
        services.AddScoped<IAppConfigService, AppConfigService>();
        return services;
    }
}
