using ACommerce.Chats.Abstractions.Providers;
using ACommerce.Chats.Core.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Chats.Core.Extensions;

/// <summary>
/// Extension methods for registering Chat services
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers all chat services including providers
	/// </summary>
	public static IServiceCollection AddChatServices(
		this IServiceCollection services)
	{
		// Register Message Provider
		services.AddScoped<IMessageProvider, DatabaseMessageProvider>();

		// Register Realtime Chat Provider
		services.AddScoped<IRealtimeChatProvider, SignalRRealtimeChatProvider>();

		return services;
	}
}
