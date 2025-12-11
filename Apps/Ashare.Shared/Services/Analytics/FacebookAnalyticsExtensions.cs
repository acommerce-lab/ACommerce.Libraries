using Ashare.Shared.Services.Analytics.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Ashare.Shared.Services.Analytics;

public static class FacebookAnalyticsExtensions
{
    public static IServiceCollection AddFacebookMobileAnalytics(
        this IServiceCollection services,
        string appId,
        string? accessToken = null,
        bool debugMode = false)
    {
        services.AddHttpClient<MetaMobileAnalyticsProvider>();

        services.AddSingleton<MetaMobileAnalyticsProvider>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
            var advertiserIdService = sp.GetService<IAdvertiserIdService>();

            var provider = new MetaMobileAnalyticsProvider(httpClient, advertiserIdService);

            provider.InitializeAsync(new AnalyticsConfig
            {
                AppId = appId,
                AccessToken = accessToken,
                DebugMode = debugMode
            }).GetAwaiter().GetResult();

            return provider;
        });

        services.AddSingleton<IAnalyticsProvider>(sp => sp.GetRequiredService<MetaMobileAnalyticsProvider>());

        return services;
    }
}
