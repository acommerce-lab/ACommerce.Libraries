using ACommerce.Client.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.Categories.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCategoriesClient(
		this IServiceCollection services,
		string registryUrl)
	{
		services.AddACommerceClient(registryUrl);
		services.AddScoped<CategoriesClient>();
		return services;
	}
}
