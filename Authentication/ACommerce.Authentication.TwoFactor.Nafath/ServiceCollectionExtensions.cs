using ACommerce.Authentication.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Authentication.TwoFactor.Nafath;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Nafath authentication provider with default configuration
    /// </summary>
    public static IServiceCollection AddNafathAuthentication(
        this IServiceCollection services)
    {
        // Register HTTP client for Nafath API
        services.AddHttpClient<INafathApiClient, NafathApiClient>();

        // Register Nafath provider
        services.AddScoped<NafathAuthenticationProvider>();
        services.AddScoped<ITwoFactorAuthenticationProvider>(sp =>
            sp.GetRequiredService<NafathAuthenticationProvider>());

        return services;
    }
}