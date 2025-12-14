using ACommerce.Templates.Customer.Services.Analytics.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ACommerce.Templates.Customer.Services.Analytics;

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
    public static IServiceCollection AddACommerceAnalytics(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useMockInDebug = true)
    {
        services.Configure<AnalyticsOptions>(
            configuration.GetSection(AnalyticsOptions.SectionName));

#if DEBUG
        if (useMockInDebug)
        {
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

        services.AddScoped<MetaAnalyticsProvider>();
        services.AddScoped<GoogleAnalyticsProvider>();
        services.AddScoped<TikTokAnalyticsProvider>();
        services.AddScoped<SnapchatAnalyticsProvider>();

        services.AddScoped<AnalyticsService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AnalyticsOptions>>();
            var service = new AnalyticsService(options);

            service.AddProvider(sp.GetRequiredService<MetaAnalyticsProvider>());
            service.AddProvider(sp.GetRequiredService<GoogleAnalyticsProvider>());
            service.AddProvider(sp.GetRequiredService<TikTokAnalyticsProvider>());
            service.AddProvider(sp.GetRequiredService<SnapchatAnalyticsProvider>());

            return service;
        });

        return services;
    }

    /// <summary>
    /// Add analytics services with manual configuration (for MAUI/Mobile apps)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Action to configure analytics options</param>
    /// <param name="useMockProvider">Use Mock provider instead of real providers</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddACommerceAnalytics(
        this IServiceCollection services,
        Action<AnalyticsOptions> configureOptions,
        bool useMockProvider = false)
    {
        services.Configure(configureOptions);

        if (useMockProvider)
        {
            Console.WriteLine("🧪 [Analytics] Mock Mode ENABLED");
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

        services.AddScoped<MetaAnalyticsProvider>();
        services.AddScoped<GoogleAnalyticsProvider>();
        services.AddScoped<TikTokAnalyticsProvider>();
        services.AddScoped<SnapchatAnalyticsProvider>();

        services.AddScoped<AnalyticsService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AnalyticsOptions>>();
            var service = new AnalyticsService(options);

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
