using ACommerce.Marketing.TikTokConversions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Marketing.TikTokConversions.Extensions;

public static class TikTokConversionsExtensions
{
    public static IServiceCollection AddTikTokConversions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TikTokConversionsOptions>(
            configuration.GetSection(TikTokConversionsOptions.SectionName));

        services.AddHttpClient<ITikTokConversionsService, TikTokConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    public static IServiceCollection AddTikTokConversions(
        this IServiceCollection services,
        Action<TikTokConversionsOptions> configure)
    {
        services.Configure(configure);

        services.AddHttpClient<ITikTokConversionsService, TikTokConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
