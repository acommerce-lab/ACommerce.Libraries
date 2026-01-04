using ACommerce.Marketing.TikTokConversions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace ACommerce.Marketing.TikTokConversions.Extensions;

public static class TikTokConversionsExtensions
{
    public static IServiceCollection AddTikTokConversions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TikTokConversionsOptions>(
            configuration.GetSection(TikTokConversionsOptions.SectionName));

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == 429)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

        services.AddHttpClient<ITikTokConversionsService, TikTokConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreaker);

        return services;
    }

    public static IServiceCollection AddTikTokConversions(
        this IServiceCollection services,
        Action<TikTokConversionsOptions> configure)
    {
        services.Configure(configure);

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == 429)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

        services.AddHttpClient<ITikTokConversionsService, TikTokConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreaker);

        return services;
    }
}