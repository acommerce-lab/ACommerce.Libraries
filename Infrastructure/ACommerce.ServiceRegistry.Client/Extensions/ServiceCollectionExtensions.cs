using ACommerce.ServiceRegistry.Client.Cache;
using ACommerce.ServiceRegistry.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.ServiceRegistry.Client.Extensions;

/// <summary>
/// Extensions لتسجيل Service Registry Client
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// إضافة Service Registry Client
	/// </summary>
	/// <param name="services">DI Container</param>
	/// <param name="registryUrl">عنوان Service Registry (مثل: http://localhost:5100)</param>
	/// <param name="configureOptions">خيارات التسجيل التلقائي (اختياري)</param>
	public static IServiceCollection AddServiceRegistryClient(
		this IServiceCollection services,
		string registryUrl,
		Action<ServiceRegistrationOptions>? configureOptions = null)
	{
		// Memory Cache
		services.AddMemoryCache();
		services.AddSingleton<ServiceCache>();

		// HttpClient for Registry
		services.AddHttpClient<ServiceRegistryClient>(client =>
		{
			client.BaseAddress = new Uri(registryUrl);
			client.Timeout = TimeSpan.FromSeconds(10);
		});

		// Options
		if (configureOptions != null)
		{
			services.Configure(configureOptions);
			services.AddHostedService<ServiceRegistrationHostedService>();
		}

		return services;
	}
}
