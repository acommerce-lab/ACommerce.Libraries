using ACommerce.Marketing.MetaConversions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace ACommerce.Marketing.MetaConversions.Extensions;

public static class MetaConversionsExtensions
{
    public static IServiceCollection AddMetaConversions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MetaConversionsOptions>(
            configuration.GetSection(MetaConversionsOptions.SectionName));

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == 429)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

        services.AddHttpClient<IMetaConversionsService, MetaConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreaker);

        return services;
    }

    public static IServiceCollection AddMetaConversions(
        this IServiceCollection services,
        Action<MetaConversionsOptions> configure)
    {
        services.Configure(configure);

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == 429)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

        services.AddHttpClient<IMetaConversionsService, MetaConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreaker);

        return services;
    }
}