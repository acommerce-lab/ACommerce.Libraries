using ACommerce.Marketing.Analytics.Services;
using ACommerce.Marketing.GoogleConversions.Extensions;
using ACommerce.Marketing.TikTokConversions.Extensions;
using ACommerce.Marketing.SnapchatConversions.Extensions;
using ACommerce.Marketing.TwitterConversions.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Marketing.Analytics.Extensions;

public static class MarketingAnalyticsExtensions
{
    /// <summary>
    /// إضافة خدمات تحليلات التسويق مع معالجة خلفية
    /// الأحداث تُضاف للطابور وتُعالج في الخلفية بدون حجب
    /// </summary>
    public static IServiceCollection AddMarketingAnalytics(this IServiceCollection services, IConfiguration configuration)
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

        // إضافة خدمات منصات الإعلانات
        services.AddGoogleConversions(configuration);
        services.AddTikTokConversions(configuration);
        services.AddSnapchatConversions(configuration);
        services.AddTwitterConversions(configuration);

        return services;
    }

    /// <summary>
    /// إضافة خدمات تحليلات التسويق بدون معالجة خلفية (للتوافق مع الإصدارات السابقة)
    /// تحذير: هذا سيحجب العمليات أثناء إرسال الأحداث
    /// </summary>
    public static IServiceCollection AddMarketingAnalyticsSync(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAttributionService, AttributionService>();
        services.AddScoped<IMarketingStatsService, MarketingStatsService>();
        services.AddScoped<IMarketingEventTracker, MarketingEventTracker>();

        // إضافة خدمات منصات الإعلانات
        services.AddGoogleConversions(configuration);
        services.AddTikTokConversions(configuration);
        services.AddSnapchatConversions(configuration);
        services.AddTwitterConversions(configuration);

        return services;
    }
}
