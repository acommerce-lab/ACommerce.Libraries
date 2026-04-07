using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Channels.Sms.Gateways;
using ACommerce.Notifications.Channels.Sms.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Notifications.Channels.Sms.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// يضيف قناة SMS مع بوابة HTTP العامة.
    ///
    /// الاستخدام:
    ///   services.AddSmsNotificationChannel(configuration);
    ///   // أو
    ///   services.AddSmsNotificationChannel(opt => { opt.SenderNumber = "+966..."; ... });
    /// </summary>
    public static IServiceCollection AddSmsNotificationChannel(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = SmsOptions.SectionName)
    {
        var options = configuration
            .GetSection(sectionName)
            .Get<SmsOptions>() ?? new SmsOptions { SenderNumber = "ACommerce" };

        return services.AddSmsNotificationChannel(options);
    }

    /// <summary>يضيف قناة SMS مع خيارات مخصصة</summary>
    public static IServiceCollection AddSmsNotificationChannel(
        this IServiceCollection services,
        Action<SmsOptions> configure)
    {
        var options = new SmsOptions { SenderNumber = "ACommerce" };
        configure(options);
        return services.AddSmsNotificationChannel(options);
    }

    /// <summary>يضيف قناة SMS مع بوابة مخصصة (Twilio, Unifonic, etc.)</summary>
    public static IServiceCollection AddSmsNotificationChannel<TGateway>(
        this IServiceCollection services,
        Action<SmsOptions> configure)
        where TGateway : class, ISmsGateway
    {
        var options = new SmsOptions { SenderNumber = "ACommerce" };
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<ISmsGateway, TGateway>();
        services.AddSingleton<INotificationChannel, SmsNotificationChannel>();

        return services;
    }

    private static IServiceCollection AddSmsNotificationChannel(
        this IServiceCollection services,
        SmsOptions options)
    {
        services.AddSingleton(options);
        services.AddHttpClient<HttpSmsGateway>()
            .ConfigureHttpClient(c => c.Timeout = options.Http.Timeout);

        services.AddSingleton<ISmsGateway, HttpSmsGateway>();
        services.AddSingleton<INotificationChannel, SmsNotificationChannel>();

        return services;
    }
}
