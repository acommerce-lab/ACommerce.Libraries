using ACommerce.Client.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.Orders.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOrdersClient(
		this IServiceCollection services,
		string registryUrl)
	{
		services.AddACommerceClient(registryUrl);
		services.AddScoped<OrdersClient>();
		return services;
	}
}
