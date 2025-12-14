using ACommerce.Authentication.Messaging.Handlers;
using ACommerce.Authentication.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Authentication.Messaging.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds authentication messaging with default configuration
    /// </summary>
    public static IServiceCollection AddAuthenticationMessaging(
        this IServiceCollection services)
    {
        services.Configure<AuthenticationMessagingOptions>(options => { });
        services.AddHostedService<AuthenticationMessagingHandler>();
        return services;
    }

    /// <summary>
    /// Adds authentication messaging with configuration from IConfiguration
    /// </summary>
    public static IServiceCollection AddAuthenticationMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthenticationMessagingOptions>(
            configuration.GetSection("AuthenticationMessaging"));
        services.AddHostedService<AuthenticationMessagingHandler>();
        return services;
    }

    /// <summary>
    /// Adds authentication messaging with custom configuration
    /// </summary>
    public static IServiceCollection AddAuthenticationMessaging(
        this IServiceCollection services,
        Action<AuthenticationMessagingOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddHostedService<AuthenticationMessagingHandler>();
        return services;
    }
}