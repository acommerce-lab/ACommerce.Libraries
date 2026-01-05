using ACommerce.Marketing.GoogleConversions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Marketing.GoogleConversions.Extensions;

public static class GoogleConversionsExtensions
{
    public static IServiceCollection AddGoogleConversions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GoogleConversionsOptions>(
            configuration.GetSection(GoogleConversionsOptions.SectionName));

        services.AddHttpClient<IGoogleConversionsService, GoogleConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    public static IServiceCollection AddGoogleConversions(
        this IServiceCollection services,
        Action<GoogleConversionsOptions> configure)
    {
        services.Configure(configure);

        services.AddHttpClient<IGoogleConversionsService, GoogleConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
