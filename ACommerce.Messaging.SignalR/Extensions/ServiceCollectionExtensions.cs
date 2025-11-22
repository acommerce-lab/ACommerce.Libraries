using ACommerce.Messaging.Abstractions.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Messaging.SignalR.Extensions;

/// <summary>
/// Extension methods for adding SignalR messaging to services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SignalR-based messaging services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="serviceName">Name of the service</param>
    public static IServiceCollection AddSignalRMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var options = new SignalRMessagingOptions
        {
            ServiceName = serviceName,
            MessagingServiceUrl = configuration["Messaging:ServiceUrl"]
                ?? configuration["Messaging:ServiceUrl"]
                ?? "http://localhost:5001"
        };

        services.AddSingleton(options);
        services.AddSingleton<IMessagePublisher, SignalRMessagePublisher>();
        services.AddSingleton<IMessageConsumer, SignalRMessageConsumer>();

        return services;
    }

    /// <summary>
    /// Adds SignalR-based messaging services with custom options
    /// </summary>
    public static IServiceCollection AddSignalRMessaging(
        this IServiceCollection services,
        Action<SignalRMessagingOptions> configureOptions)
    {
        var options = new SignalRMessagingOptions
        {
            ServiceName = "unknown",
            MessagingServiceUrl = "http://localhost:5001"
        };

        configureOptions(options);

        services.AddSingleton(options);
        services.AddSingleton<IMessagePublisher, SignalRMessagePublisher>();
        services.AddSingleton<IMessageConsumer, SignalRMessageConsumer>();

        return services;
    }
}