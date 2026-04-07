using ACommerce.Realtime.Abstractions.Contracts;
using ACommerce.Realtime.Operations.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Realtime.Providers.SignalR.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// يسجل SignalRRealtimeTransport كمزود لـ IRealtimeTransport و IConnectionTracker.
    ///
    /// الاستخدام:
    ///   services.AddSignalRRealtimeTransport&lt;NotificationHub, INotificationClient&gt;();
    /// </summary>
    public static IServiceCollection AddSignalRRealtimeTransport<THub, TClient>(
        this IServiceCollection services)
        where THub : Hub<TClient>
        where TClient : class, IRealtimeClient
    {
        // يتطلب: services.AddSignalR() قبل هذا الاستدعاء
        services.AddSingleton<IRealtimeTransport, SignalRRealtimeTransport<THub, TClient>>();
        services.AddSingleton<IConnectionTracker, InMemoryConnectionTracker>();
        return services;
    }

    /// <summary>
    /// يسجل SignalRRealtimeTransport مع خيار تخصيص tracker.
    /// </summary>
    public static IServiceCollection AddSignalRRealtimeTransport<THub, TClient>(
        this IServiceCollection services,
        Func<IServiceProvider, IConnectionTracker> trackerFactory)
        where THub : Hub<TClient>
        where TClient : class, IRealtimeClient
    {
        services.AddSingleton<IRealtimeTransport, SignalRRealtimeTransport<THub, TClient>>();
        services.AddSingleton(trackerFactory);
        return services;
    }
}
