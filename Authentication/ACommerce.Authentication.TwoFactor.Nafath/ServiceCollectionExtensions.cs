using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.Abstractions.Contracts;
using ACommerce.Authentication.TwoFactor.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.Authentication.TwoFactor.Nafath;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNafathTwoFactor(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options
        services.AddOptions<NafathOptions>()
            .Bind(configuration.GetSection(NafathOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // HttpClient
        services.AddHttpClient(NafathOptions.HttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NafathOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // ✅ API Client
        services.AddScoped<INafathApiClient, NafathApiClient>();

        // Session Store (In-Memory للتطوير - استبدل بـ Redis للإنتاج)
        //services.AddSingleton<ITwoFactorSessionStore, InMemoryTwoFactorSessionStore>();

        // ✅ Provider مع EventPublisher
        services.AddScoped<ITwoFactorAuthenticationProvider>(sp =>
        {
            var apiClient = sp.GetRequiredService<INafathApiClient>();
            var sessionStore = sp.GetRequiredService<ITwoFactorSessionStore>();
            var logger = sp.GetRequiredService<ILogger<NafathAuthenticationProvider>>();

            var provider = new NafathAuthenticationProvider(
                apiClient,
                sessionStore,
                logger);

            // ✅ تلقائياً ربط EventPublisher إذا كان موجود
            var eventPublisher = sp.GetService<IAuthenticationEventPublisher>();
            if (eventPublisher != null)
            {
                provider.SetEventPublisher(eventPublisher);
            }

            return provider;
        });

        return services;
    }
}