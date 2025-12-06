using Ashare.Shared.Services.Analytics.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ashare.Shared.Services.Analytics;

/// <summary>
/// Extension methods for registering analytics services
/// </summary>
public static class AnalyticsExtensions
{
    /// <summary>
    /// Add analytics services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration with Analytics section</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddAshareAnalytics(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<AnalyticsOptions>(
            configuration.GetSection(AnalyticsOptions.SectionName));

        // Register providers
        services.AddScoped<MetaAnalyticsProvider>();
        services.AddScoped<GoogleAnalyticsProvider>();
        services.AddScoped<TikTokAnalyticsProvider>();
        services.AddScoped<SnapchatAnalyticsProvider>();

        // Register aggregated service
        services.AddScoped<AnalyticsService>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AnalyticsOptions>>();
            var service = new AnalyticsService(options);

            // Add all providers
            service.AddProvider(sp.GetRequiredService<MetaAnalyticsProvider>());
            service.AddProvider(sp.GetRequiredService<GoogleAnalyticsProvider>());
            service.AddProvider(sp.GetRequiredService<TikTokAnalyticsProvider>());
            service.AddProvider(sp.GetRequiredService<SnapchatAnalyticsProvider>());

            return service;
        });

        return services;
    }
}
