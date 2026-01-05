using ACommerce.ServiceRegistry.Abstractions.Models;
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

	/// <summary>
	/// إضافة Service Registry Client مع خدمات محددة مسبقاً (للتطبيقات المستقلة مثل MAUI)
	/// لا يحتاج لـ Registry Server - يستخدم Cache محلي فقط
	/// </summary>
	/// <param name="services">DI Container</param>
	/// <param name="configureServices">تهيئة الخدمات المحددة مسبقاً</param>
	public static IServiceCollection AddServiceRegistryWithPredefinedServices(
		this IServiceCollection services,
		Action<PredefinedServicesOptions> configureServices)
	{
		var options = new PredefinedServicesOptions();
		configureServices(options);

		// Memory Cache
		services.AddMemoryCache();
		services.AddSingleton<ServiceCache>();

		// تسجيل الخدمات المحددة مسبقاً عند بدء التشغيل
		services.AddSingleton<IServiceCacheInitializer>(sp =>
			new PredefinedServicesCacheInitializer(options.Services));

		// HttpClient بدون Registry (placeholder)
		services.AddHttpClient<ServiceRegistryClient>(client =>
		{
			// لن يتم استخدامه لأن الخدمات موجودة في Cache
			client.BaseAddress = new Uri("http://localhost");
			client.Timeout = TimeSpan.FromSeconds(5);
		});

		return services;
	}

	/// <summary>
	/// تهيئة Cache بالخدمات المحددة مسبقاً
	/// يجب استدعاؤها عند بدء التطبيق
	/// </summary>
	public static IServiceProvider InitializeServiceCache(this IServiceProvider serviceProvider)
	{
		var initializer = serviceProvider.GetService<IServiceCacheInitializer>();
		if (initializer != null)
		{
			var cache = serviceProvider.GetRequiredService<ServiceCache>();
			initializer.Initialize(cache);
		}
		return serviceProvider;
	}
}

/// <summary>
/// خيارات الخدمات المحددة مسبقاً
/// </summary>
public class PredefinedServicesOptions
{
	internal List<ServiceEndpoint> Services { get; } = new();

	/// <summary>
	/// إضافة خدمة محددة مسبقاً
	/// </summary>
	public PredefinedServicesOptions AddService(string serviceName, string baseUrl, string version = "v1")
	{
		Services.Add(new ServiceEndpoint
		{
			Id = Guid.NewGuid().ToString(),
			ServiceName = serviceName,
			BaseUrl = baseUrl.TrimEnd('/'),
			Version = version,
			Environment = "Production",
			Weight = 100,
			Health = new ServiceHealth
			{
				Status = HealthStatus.Healthy,
				LastChecked = DateTime.UtcNow
			},
			RegisteredAt = DateTime.UtcNow,
			LastUpdated = DateTime.UtcNow
		});
		return this;
	}
}

/// <summary>
/// واجهة لتهيئة Cache
/// </summary>
public interface IServiceCacheInitializer
{
	void Initialize(ServiceCache cache);
}

/// <summary>
/// تنفيذ تهيئة Cache بالخدمات المحددة مسبقاً
/// </summary>
internal class PredefinedServicesCacheInitializer : IServiceCacheInitializer
{
	private readonly List<ServiceEndpoint> _services;

	public PredefinedServicesCacheInitializer(List<ServiceEndpoint> services)
	{
		_services = services;
	}

	public void Initialize(ServiceCache cache)
	{
		foreach (var service in _services)
		{
			// استخدام SetPermanent للخدمات المحددة مسبقاً حتى لا تنتهي صلاحيتها
			cache.SetPermanent(service.ServiceName, service);
		}
	}
}
