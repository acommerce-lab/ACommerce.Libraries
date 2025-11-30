using ACommerce.Client.Core.Http;
using ACommerce.Client.Core.Interceptors;
using ACommerce.ServiceRegistry.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ACommerce.Client.Core.Extensions;

/// <summary>
/// Extensions لتسجيل ACommerce Client SDK
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// إضافة ACommerce Client Core مع Service Registry (للخدمات الموزعة)
	/// </summary>
	public static IServiceCollection AddACommerceClient(
		this IServiceCollection services,
		string registryUrl,
		Action<ClientOptions>? configureOptions = null)
	{
		// Options
		var options = new ClientOptions();
		configureOptions?.Invoke(options);

		// ✨ Service Registry Client (مع Cache تلقائي)
		services.AddServiceRegistryClient(registryUrl);

		// Localization Provider
		if (options.LocalizationProvider != null)
		{
			services.AddSingleton(options.LocalizationProvider);
		}
		else if (options.EnableLocalization)
		{
			services.AddSingleton<ILocalizationProvider, DefaultLocalizationProvider>();
		}

		// HttpClient مع Interceptors
		var httpClientBuilder = services.AddHttpClient<DynamicHttpClient>()
			.ConfigureHttpClient(client =>
			{
				client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
			});

		// Localization Interceptor
		if (options.EnableLocalization)
		{
			httpClientBuilder.AddHttpMessageHandler<LocalizationInterceptor>();
		}

		// Authentication Interceptor
		if (options.EnableAuthentication && options.TokenProvider != null)
		{
			services.AddScoped<ITokenProvider>(options.TokenProvider);
			httpClientBuilder.AddHttpMessageHandler<AuthenticationInterceptor>();
		}

		// Retry Interceptor
		if (options.EnableRetry)
		{
			httpClientBuilder.AddHttpMessageHandler(sp =>
				new RetryInterceptor(
					sp.GetRequiredService<ILogger<RetryInterceptor>>(),
					options.MaxRetries));
		}

		// Register IApiClient as DynamicHttpClient
		services.AddScoped<IApiClient>(sp => sp.GetRequiredService<DynamicHttpClient>());

		return services;
	}

	/// <summary>
	/// إضافة ACommerce Client Core مع Static URL (للتطبيقات المستقلة مثل MAUI و Blazor WASM)
	/// </summary>
	public static IServiceCollection AddACommerceStaticClient(
		this IServiceCollection services,
		string baseUrl,
		Action<StaticClientOptions>? configureOptions = null)
	{
		// Options
		var options = new StaticClientOptions();
		configureOptions?.Invoke(options);

		// HttpClient
		services.AddHttpClient("ACommerceApi", client =>
		{
			client.BaseAddress = new Uri(baseUrl);
			client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
		});

		// StaticHttpClient كـ IApiClient
		services.AddScoped<IApiClient>(sp =>
		{
			var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient("ACommerceApi");
			var logger = sp.GetService<ILogger<StaticHttpClient>>();
			return new StaticHttpClient(httpClient, baseUrl, logger);
		});

		return services;
	}
}

/// <summary>
/// خيارات ACommerce Client
/// </summary>
public class ClientOptions
{
	/// <summary>
	/// Timeout بالثواني (افتراضياً: 30)
	/// </summary>
	public int TimeoutSeconds { get; set; } = 30;

	/// <summary>
	/// تفعيل Retry تلقائي؟ (افتراضياً: true)
	/// </summary>
	public bool EnableRetry { get; set; } = true;

	/// <summary>
	/// عدد المحاولات (افتراضياً: 3)
	/// </summary>
	public int MaxRetries { get; set; } = 3;

	/// <summary>
	/// تفعيل Authentication تلقائي؟ (افتراضياً: false)
	/// </summary>
	public bool EnableAuthentication { get; set; } = false;

	/// <summary>
	/// Token Provider للـ Authentication
	/// </summary>
	public Func<IServiceProvider, ITokenProvider>? TokenProvider { get; set; }

	/// <summary>
	/// تفعيل Localization تلقائي؟ (افتراضياً: true)
	/// </summary>
	public bool EnableLocalization { get; set; } = true;

	/// <summary>
	/// Localization Provider مخصص (اختياري)
	/// </summary>
	public Func<IServiceProvider, ILocalizationProvider>? LocalizationProvider { get; set; }
}

/// <summary>
/// خيارات ACommerce Client للتطبيقات المستقلة (Static URL)
/// </summary>
public class StaticClientOptions
{
	/// <summary>
	/// Timeout بالثواني (افتراضياً: 30)
	/// </summary>
	public int TimeoutSeconds { get; set; } = 30;
}
