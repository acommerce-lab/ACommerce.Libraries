using ACommerce.Messaging.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ACommerce.Messaging.InMemory.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add in-memory messaging (for development/testing)
    /// </summary>
    public static IServiceCollection AddInMemoryMessaging(
        this IServiceCollection services,
        string serviceName)
    {
        // Register as singleton (shared across all services in same process)
        services.AddSingleton<IMessageBus>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<InMemoryMessageBus>>();
            return new InMemoryMessageBus(logger, serviceName);
        });

        // Register individual interfaces
        services.AddSingleton<IMessagePublisher>(sp =>
            sp.GetRequiredService<IMessageBus>());

        services.AddSingleton<IMessageConsumer>(sp =>
            sp.GetRequiredService<IMessageBus>());

        services.AddSingleton<IMessageRequestor>(sp =>
            (IMessageRequestor)sp.GetRequiredService<IMessageBus>());

        return services;
    }
}