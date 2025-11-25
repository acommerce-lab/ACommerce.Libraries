using ACommerce.Client.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.Products.Extensions;

/// <summary>
/// Extensions لتسجيل Products Client
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// إضافة Products Client
	/// </summary>
	public static IServiceCollection AddProductsClient(
		this IServiceCollection services,
		string registryUrl)
	{
		// ACommerce Client (إذا لم يكن مسجلاً مسبقاً)
		services.AddACommerceClient(registryUrl);

		// Products Client
		services.AddScoped<ProductsClient>();

		return services;
	}
}
