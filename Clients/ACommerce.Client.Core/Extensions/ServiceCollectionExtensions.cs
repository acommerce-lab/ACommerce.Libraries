using ACommerce.Client.Core.Http;
using ACommerce.Client.Core.Interceptors;
using ACommerce.ServiceRegistry.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.Core.Extensions;

/// <summary>
/// Extensions لتسجيل ACommerce Client SDK
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// إضافة ACommerce Client Core مع Service Registry
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

		// HttpClient مع Interceptors
		var httpClientBuilder = services.AddHttpClient<DynamicHttpClient>()
			.ConfigureHttpClient(client =>
			{
				client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
			});

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
					sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RetryInterceptor>>(),
					options.MaxRetries));
		}

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
}
