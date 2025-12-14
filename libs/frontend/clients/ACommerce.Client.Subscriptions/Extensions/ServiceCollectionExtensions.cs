using ACommerce.Client.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.Subscriptions.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSubscriptionsClient(
		this IServiceCollection services,
		string registryUrl)
	{
		services.AddACommerceClient(registryUrl);
		services.AddScoped<SubscriptionClient>();
		return services;
	}
}
