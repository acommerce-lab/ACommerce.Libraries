using ACommerce.Authentication.Abstractions.Contracts;
using ACommerce.Authentication.Messaging.Handlers;
using ACommerce.Authentication.Messaging.Publishers;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Authentication.Messaging.Extensions;

/// <summary>
/// Extension methods for adding authentication messaging services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds authentication messaging integration (event publishing + notification handling)
    /// </summary>
    public static IServiceCollection AddAuthenticationMessaging(
        this IServiceCollection services)
    {
        // Register event publisher
        services.AddScoped<IAuthenticationEventPublisher, MessagingAuthenticationEventPublisher>();

        // Register background handler
        services.AddHostedService<AuthenticationMessagingHandler>();

        return services;
    }
}