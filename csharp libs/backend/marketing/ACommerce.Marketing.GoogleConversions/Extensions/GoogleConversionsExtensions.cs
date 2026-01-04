using ACommerce.Marketing.GoogleConversions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;

namespace ACommerce.Marketing.GoogleConversions.Extensions;

public static class GoogleConversionsExtensions
{
    public static IServiceCollection AddGoogleConversions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GoogleConversionsOptions>(
            configuration.GetSection(GoogleConversionsOptions.SectionName));

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == 429)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

        services.AddHttpClient<IGoogleConversionsService, GoogleConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreaker);

        return services;
    }

    public static IServiceCollection AddGoogleConversions(
        this IServiceCollection services,
        Action<GoogleConversionsOptions> configure)
    {
        services.Configure(configure);

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == 429)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

        services.AddHttpClient<IGoogleConversionsService, GoogleConversionsService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreaker);

        return services;
    }
}