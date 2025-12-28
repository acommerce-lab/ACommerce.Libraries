using ACommerce.Marketing.Analytics.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Marketing.Analytics.Extensions;

public static class MarketingAnalyticsExtensions
{
    /// <summary>
    /// إضافة خدمات تحليلات التسويق مع معالجة خلفية
    /// الأحداث تُضاف للطابور وتُعالج في الخلفية بدون حجب
    /// </summary>
    public static IServiceCollection AddMarketingAnalytics(this IServiceCollection services)
    {
        // خدمات أساسية
        services.AddScoped<IAttributionService, AttributionService>();
        services.AddScoped<IMarketingStatsService, MarketingStatsService>();

        // طابور الأحداث (Singleton لأنه يُشارك بين الطلبات)
        services.AddSingleton<IMarketingEventQueue, MarketingEventQueue>();

        // معالج الأحداث الفعلي (Scoped لأنه يستخدم DbContext)
        services.AddScoped<IMarketingEventProcessor, MarketingEventTracker>();

        // الخدمة التي تستخدمها Controllers (تضع في الطابور فقط)
        services.AddScoped<IMarketingEventTracker, BackgroundMarketingEventTracker>();

        // خدمة خلفية لمعالجة الطابور
        services.AddHostedService<MarketingEventBackgroundService>();

        return services;
    }

    /// <summary>
    /// إضافة خدمات تحليلات التسويق بدون معالجة خلفية (للتوافق مع الإصدارات السابقة)
    /// تحذير: هذا سيحجب العمليات أثناء إرسال الأحداث
    /// </summary>
    public static IServiceCollection AddMarketingAnalyticsSync(this IServiceCollection services)
    {
        services.AddScoped<IAttributionService, AttributionService>();
        services.AddScoped<IMarketingStatsService, MarketingStatsService>();
        services.AddScoped<IMarketingEventTracker, MarketingEventTracker>();

        return services;
    }
}
