using ACommerce.Client.AppConfig.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.AppConfig.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// تسجيل AppConfig client + store + bootstrapper.
    /// يجب استدعاء <see cref="AppConfigBootstrapper.InitializeAsync"/> بعد بناء الـ host
    /// (مرة واحدة عند بدء التطبيق).
    /// </summary>
    public static IServiceCollection AddACommerceAppConfigClient(
        this IServiceCollection services,
        Action<AppConfigClientOptions>? configure = null)
    {
        var options = new AppConfigClientOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddSingleton<AppConfigStore>();
        services.AddScoped<AppConfigClient>();
        services.AddSingleton<AppConfigBootstrapper>();
        services.AddSingleton<IFeatureFlags, StoreBackedFeatureFlags>();

        return services;
    }
}
