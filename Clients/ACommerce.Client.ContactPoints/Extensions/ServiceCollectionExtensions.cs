using ACommerce.Client.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.ContactPoints.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddContactPointsClient(
		this IServiceCollection services,
		string registryUrl)
	{
		services.AddACommerceClient(registryUrl);
		services.AddScoped<ContactPointsClient>();
		return services;
	}
}
