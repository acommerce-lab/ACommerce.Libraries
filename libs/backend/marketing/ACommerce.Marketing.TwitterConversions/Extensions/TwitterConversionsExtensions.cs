using ACommerce.Marketing.TwitterConversions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Marketing.TwitterConversions.Extensions;

public static class TwitterConversionsExtensions
{
    public static IServiceCollection AddTwitterConversions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TwitterConversionsOptions>(
            configuration.GetSection(TwitterConversionsOptions.SectionName));

        services.AddHttpClient<ITwitterConversionsService, TwitterConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    public static IServiceCollection AddTwitterConversions(
        this IServiceCollection services,
        Action<TwitterConversionsOptions> configure)
    {
        services.Configure(configure);

        services.AddHttpClient<ITwitterConversionsService, TwitterConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
