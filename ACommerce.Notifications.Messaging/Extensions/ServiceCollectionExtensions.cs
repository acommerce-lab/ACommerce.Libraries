using ACommerce.Notifications.Messaging.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Notifications.Messaging.Extensions;

/// <summary>
/// Extension methods for adding notification messaging services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds notification messaging integration (command handling)
    /// </summary>
    public static IServiceCollection AddNotificationMessaging(
        this IServiceCollection services)
    {
        // Register background handler
        services.AddHostedService<NotificationMessagingHandler>();

        return services;
    }
}