using ACommerce.Marketing.SnapchatConversions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace ACommerce.Marketing.SnapchatConversions.Extensions;

public static class SnapchatConversionsExtensions
{
    public static IServiceCollection AddSnapchatConversions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SnapchatConversionsOptions>(
            configuration.GetSection(SnapchatConversionsOptions.SectionName));

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == 429)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

        services.AddHttpClient<ISnapchatConversionsService, SnapchatConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreaker);

        return services;
    }

    public static IServiceCollection AddSnapchatConversions(
        this IServiceCollection services,
        Action<SnapchatConversionsOptions> configure)
    {
        services.Configure(configure);

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == 429)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

        services.AddHttpClient<ISnapchatConversionsService, SnapchatConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreaker);

        return services;
    }
}