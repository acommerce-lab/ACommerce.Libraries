using ACommerce.Marketing.SnapchatConversions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Marketing.SnapchatConversions.Extensions;

public static class SnapchatConversionsExtensions
{
    public static IServiceCollection AddSnapchatConversions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SnapchatConversionsOptions>(
            configuration.GetSection(SnapchatConversionsOptions.SectionName));

        services.AddHttpClient<ISnapchatConversionsService, SnapchatConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    public static IServiceCollection AddSnapchatConversions(
        this IServiceCollection services,
        Action<SnapchatConversionsOptions> configure)
    {
        services.Configure(configure);

        services.AddHttpClient<ISnapchatConversionsService, SnapchatConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
