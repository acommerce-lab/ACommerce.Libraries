using ACommerce.Realtime.Abstractions.Contracts;
using ACommerce.Realtime.SignalR.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Realtime.SignalR.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds generic SignalR hub services
	/// </summary>
	public static IServiceCollection AddACommerceSignalR<THub, TClient>(
		this IServiceCollection services,
		Action<HubOptions>? configureHub = null)
		where THub : Hub<TClient>
		where TClient : class, IRealtimeClient
	{
		services.AddSignalR(options =>
		{
			configureHub?.Invoke(options);
		});

		services.AddScoped<IRealtimeHub, SignalRRealtimeHub<THub, TClient>>();

		return services;
	}
}

