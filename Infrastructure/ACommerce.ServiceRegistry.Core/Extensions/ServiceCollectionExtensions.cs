using ACommerce.ServiceRegistry.Abstractions.Interfaces;
using ACommerce.ServiceRegistry.Core.Services;
using ACommerce.ServiceRegistry.Core.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.ServiceRegistry.Core.Extensions;

/// <summary>
/// Extensions لتسجيل Service Registry في DI Container
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// إضافة Service Registry Core Services
	/// </summary>
	public static IServiceCollection AddServiceRegistryCore(this IServiceCollection services)
	{
		// Storage
		services.AddSingleton<IServiceStore, InMemoryServiceStore>();

		// Services
		services.AddSingleton<IServiceRegistry, Services.ServiceRegistry>();
		services.AddSingleton<IServiceDiscovery, ServiceDiscovery>();
		services.AddSingleton<IHealthChecker, HealthChecker>();

		// HttpClient for Health Checks
		services.AddHttpClient();

		return services;
	}
}
