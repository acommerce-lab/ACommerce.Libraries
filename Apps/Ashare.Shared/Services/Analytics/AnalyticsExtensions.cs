using Ashare.Shared.Services.Analytics.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
    /// <param name="useMockInDebug">Use Mock provider in DEBUG builds</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddAshareAnalytics(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useMockInDebug = true)
    {
        // Bind configuration
        services.Configure<AnalyticsOptions>(
            configuration.GetSection(AnalyticsOptions.SectionName));

#if DEBUG
        if (useMockInDebug)
        {
            // في وضع التطوير: استخدم Mock provider فقط
            services.AddScoped<MockAnalyticsProvider>();
            services.AddScoped<AnalyticsService>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AnalyticsOptions>>();
                var service = new AnalyticsService(options);
                service.AddProvider(sp.GetRequiredService<MockAnalyticsProvider>());
                return service;
            });
            return services;
        }
#endif

        // Register real providers
        services.AddScoped<MetaAnalyticsProvider>();
        services.AddScoped<GoogleAnalyticsProvider>();
        services.AddScoped<TikTokAnalyticsProvider>();
        services.AddScoped<SnapchatAnalyticsProvider>();

        // Register aggregated service
        services.AddScoped<AnalyticsService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AnalyticsOptions>>();
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

    /// <summary>
    /// Add ONLY mock analytics for testing (no real providers)
    /// استخدم هذا للاختبار بدون حسابات فعلية
    /// </summary>
    public static IServiceCollection AddMockAnalytics(this IServiceCollection services)
    {
        services.Configure<AnalyticsOptions>(options =>
        {
            options.Enabled = true;
            options.Meta = new AnalyticsConfig { AppId = "MOCK_META", DebugMode = true };
            options.Google = new AnalyticsConfig { AppId = "MOCK_GOOGLE", DebugMode = true };
            options.TikTok = new AnalyticsConfig { AppId = "MOCK_TIKTOK", DebugMode = true };
            options.Snapchat = new AnalyticsConfig { AppId = "MOCK_SNAPCHAT", DebugMode = true };
        });

        services.AddScoped<MockAnalyticsProvider>();
        services.AddScoped<AnalyticsService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AnalyticsOptions>>();
            var service = new AnalyticsService(options);
            service.AddProvider(sp.GetRequiredService<MockAnalyticsProvider>());
            return service;
        });

        return services;
    }
}
