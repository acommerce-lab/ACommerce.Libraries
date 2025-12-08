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
        // إعدادات الخيارات
        var options = new ClientOptions();
        configureOptions?.Invoke(options);

        // ✨ Service Registry Client
        services.AddServiceRegistryClient(registryUrl);

        // تسجيل LocalizationProvider
        if (options.LocalizationProvider != null)
        {
            services.AddSingleton(options.LocalizationProvider);
        }
        else if (options.EnableLocalization)
        {
            services.AddSingleton<ILocalizationProvider, DefaultLocalizationProvider>();
        }

        // تسجيل Interceptors
        if (options.EnableLocalization)
        {
            services.AddTransient<LocalizationInterceptor>();
        }
        if (options.EnableAuthentication && options.TokenProvider != null)
        {
            // ITokenProvider must be Singleton to share token across IHttpClientFactory scopes
            services.AddSingleton<ITokenProvider>(options.TokenProvider);
            services.AddTransient<AuthenticationInterceptor>();
        }
        if (options.EnableRetry)
        {
            services.AddTransient<RetryInterceptor>();
        }

        // HttpClient مع Interceptors
        var httpClientBuilder = services.AddHttpClient<DynamicHttpClient>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        // إضافة Interceptors إلى HttpClient
        if (options.EnableLocalization)
            httpClientBuilder.AddHttpMessageHandler<LocalizationInterceptor>();

        if (options.EnableAuthentication && options.TokenProvider != null)
            httpClientBuilder.AddHttpMessageHandler<AuthenticationInterceptor>();

        if (options.EnableRetry)
        {
            httpClientBuilder.AddHttpMessageHandler(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RetryInterceptor>>();
                return new RetryInterceptor(logger, options.MaxRetries);
            });
        }

        // تسجيل IApiClient
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
        var options = new StaticClientOptions();
        configureOptions?.Invoke(options);

        // HttpClient باسم ثابت
        services.AddHttpClient("ACommerceApi", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        // تسجيل StaticHttpClient كـ IApiClient
        services.AddScoped<IApiClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("ACommerceApi");
            var logger = sp.GetService<ILogger<StaticHttpClient>>();
            return new StaticHttpClient(httpClient, baseUrl, logger);
        });

        return services;
    }

    /// <summary>
    /// إضافة ACommerce Client مع خدمات محددة مسبقاً (للتطبيقات المستقلة مثل MAUI)
    /// يستخدم DynamicHttpClient مع Service Cache محلي بدون الحاجة لـ Registry Server
    /// </summary>
    /// <param name="services">DI Container</param>
    /// <param name="configureServices">تهيئة الخدمات (مثل: Marketplace, Files, etc.)</param>
    /// <param name="configureOptions">خيارات إضافية</param>
    public static IServiceCollection AddACommerceClientWithServices(
        this IServiceCollection services,
        Action<PredefinedServicesOptions> configureServices,
        Action<ClientOptions>? configureOptions = null)
    {
        var options = new ClientOptions();
        configureOptions?.Invoke(options);

        // ✨ Service Registry مع خدمات محددة مسبقاً
        services.AddServiceRegistryWithPredefinedServices(configureServices);

        // تسجيل LocalizationProvider
        if (options.LocalizationProvider != null)
        {
            services.AddSingleton(options.LocalizationProvider);
        }
        else if (options.EnableLocalization)
        {
            services.AddSingleton<ILocalizationProvider, DefaultLocalizationProvider>();
        }

        // تسجيل Interceptors
        if (options.EnableLocalization)
        {
            services.AddTransient<LocalizationInterceptor>();
        }
        if (options.EnableAuthentication && options.TokenProvider != null)
        {
            // ITokenProvider must be Singleton to share token across IHttpClientFactory scopes
            services.AddSingleton<ITokenProvider>(options.TokenProvider);
            services.AddTransient<AuthenticationInterceptor>();
        }
        if (options.EnableRetry)
        {
            services.AddTransient<RetryInterceptor>();
        }

        // HttpClient مع Interceptors
        var httpClientBuilder = services.AddHttpClient<DynamicHttpClient>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        // تجاوز التحقق من SSL في التطوير (للشهادات الذاتية)
        if (options.BypassSslValidation)
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
        }

        // إضافة Interceptors إلى HttpClient
        if (options.EnableLocalization)
            httpClientBuilder.AddHttpMessageHandler<LocalizationInterceptor>();

        if (options.EnableAuthentication && options.TokenProvider != null)
            httpClientBuilder.AddHttpMessageHandler<AuthenticationInterceptor>();

        if (options.EnableRetry)
        {
            httpClientBuilder.AddHttpMessageHandler(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RetryInterceptor>>();
                return new RetryInterceptor(logger, options.MaxRetries);
            });
        }

        // تسجيل IApiClient
        services.AddScoped<IApiClient>(sp => sp.GetRequiredService<DynamicHttpClient>());

        return services;
    }
}


/// <summary>
/// خيارات ACommerce Client
/// </summary>
public class ClientOptions
{
        /// <summary>
        /// Timeout بالثواني (افتراضياً: 60)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// تفعيل Retry تلقائي؟ (افتراضياً: true)
        /// </summary>
        public bool EnableRetry { get; set; } = true;

        /// <summary>
        /// عدد المحاولات (افتراضياً: 5)
        /// </summary>
        public int MaxRetries { get; set; } = 5;

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

        /// <summary>
        /// تجاوز التحقق من شهادة SSL (للتطوير فقط!)
        /// يسمح بالاتصال بخوادم تستخدم شهادات ذاتية
        /// </summary>
        public bool BypassSslValidation { get; set; } = false;
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
