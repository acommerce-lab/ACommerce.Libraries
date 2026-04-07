using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Channels.WhatsApp.Gateways;
using ACommerce.Notifications.Channels.WhatsApp.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Notifications.Channels.WhatsApp.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// يضيف قناة WhatsApp مع Meta Cloud API (الافتراضي).
    ///
    /// الاستخدام:
    ///   services.AddWhatsAppNotificationChannel(configuration);
    /// </summary>
    public static IServiceCollection AddWhatsAppNotificationChannel(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = WhatsAppOptions.SectionName)
    {
        var options = configuration
            .GetSection(sectionName)
            .Get<WhatsAppOptions>() ?? new WhatsAppOptions { FromNumber = "" };

        return services.AddWhatsAppNotificationChannel(options);
    }

    /// <summary>يضيف قناة WhatsApp مع خيارات مخصصة</summary>
    public static IServiceCollection AddWhatsAppNotificationChannel(
        this IServiceCollection services,
        Action<WhatsAppOptions> configure)
    {
        var options = new WhatsAppOptions { FromNumber = "" };
        configure(options);
        return services.AddWhatsAppNotificationChannel(options);
    }

    /// <summary>يضيف قناة WhatsApp مع بوابة مخصصة</summary>
    public static IServiceCollection AddWhatsAppNotificationChannel<TGateway>(
        this IServiceCollection services,
        Action<WhatsAppOptions> configure)
        where TGateway : class, IWhatsAppGateway
    {
        var options = new WhatsAppOptions { FromNumber = "" };
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<IWhatsAppGateway, TGateway>();
        services.AddSingleton<INotificationChannel, WhatsAppNotificationChannel>();

        return services;
    }

    private static IServiceCollection AddWhatsAppNotificationChannel(
        this IServiceCollection services,
        WhatsAppOptions options)
    {
        services.AddSingleton(options);
        services.AddHttpClient<CloudApiWhatsAppGateway>()
            .ConfigureHttpClient(c => c.Timeout = options.Http.Timeout);

        services.AddSingleton<IWhatsAppGateway, CloudApiWhatsAppGateway>();
        services.AddSingleton<INotificationChannel, WhatsAppNotificationChannel>();

        return services;
    }
}
