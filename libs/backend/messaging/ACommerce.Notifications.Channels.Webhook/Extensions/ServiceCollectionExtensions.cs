using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Channels.Webhook.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Notifications.Channels.Webhook.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// يضيف قناة Webhook.
    ///
    /// الاستخدام:
    ///   services.AddWebhookNotificationChannel(configuration);
    ///   // أو
    ///   services.AddWebhookNotificationChannel(opt => { opt.DefaultUrl = "https://..."; });
    /// </summary>
    public static IServiceCollection AddWebhookNotificationChannel(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = WebhookOptions.SectionName)
    {
        var options = configuration
            .GetSection(sectionName)
            .Get<WebhookOptions>() ?? new WebhookOptions();

        return services.AddWebhookNotificationChannel(options);
    }

    /// <summary>يضيف قناة Webhook مع خيارات مخصصة</summary>
    public static IServiceCollection AddWebhookNotificationChannel(
        this IServiceCollection services,
        Action<WebhookOptions> configure)
    {
        var options = new WebhookOptions();
        configure(options);
        return services.AddWebhookNotificationChannel(options);
    }

    private static IServiceCollection AddWebhookNotificationChannel(
        this IServiceCollection services,
        WebhookOptions options)
    {
        services.AddSingleton(options);
        services.AddHttpClient<WebhookNotificationChannel>()
            .ConfigureHttpClient(c => c.Timeout = options.Timeout);

        services.AddSingleton<INotificationChannel, WebhookNotificationChannel>();

        return services;
    }
}
