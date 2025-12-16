using ACommerce.Marketing.MetaConversions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Marketing.MetaConversions.Extensions;

public static class MetaConversionsExtensions
{
    public static IServiceCollection AddMetaConversions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MetaConversionsOptions>(
            configuration.GetSection(MetaConversionsOptions.SectionName));

        services.AddHttpClient<IMetaConversionsService, MetaConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    public static IServiceCollection AddMetaConversions(
        this IServiceCollection services,
        Action<MetaConversionsOptions> configure)
    {
        services.Configure(configure);

        services.AddHttpClient<IMetaConversionsService, MetaConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
