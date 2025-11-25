using ACommerce.Client.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.Cart.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCartClient(
		this IServiceCollection services,
		string registryUrl)
	{
		services.AddACommerceClient(registryUrl);
		services.AddScoped<CartClient>();
		return services;
	}
}
