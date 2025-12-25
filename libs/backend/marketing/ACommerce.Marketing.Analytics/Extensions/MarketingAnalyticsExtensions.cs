using ACommerce.Marketing.Analytics.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Marketing.Analytics.Extensions;

public static class MarketingAnalyticsExtensions
{
    /// <summary>
    /// إضافة خدمات تحليلات التسويق
    /// </summary>
    public static IServiceCollection AddMarketingAnalytics(this IServiceCollection services)
    {
        services.AddScoped<IAttributionService, AttributionService>();
        services.AddScoped<IMarketingStatsService, MarketingStatsService>();
        services.AddScoped<IMarketingEventTracker, MarketingEventTracker>();

        return services;
    }
}
