using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing; // Add this using directive
using Microsoft.AspNetCore.Builder; // Add this using directive

namespace ACommerce.Messaging.SignalR.Hub.Extensions;

/// <summary>
/// Extension methods for adding messaging hub services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SignalR messaging hub services
    /// </summary>
    public static IServiceCollection AddMessagingHub(
        this IServiceCollection services,
        Action<Microsoft.AspNetCore.SignalR.HubOptions>? configureHub = null)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);

            configureHub?.Invoke(options);
        });

        return services;
    }
}

/// <summary>
/// Extension methods for mapping messaging hub endpoint
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the messaging hub endpoint at /hubs/messaging
    /// </summary>
    public static IEndpointConventionBuilder MapMessagingHub(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/hubs/messaging")
    {
        return endpoints.MapHub<Hubs.MessagingHub>(pattern);
    }
}