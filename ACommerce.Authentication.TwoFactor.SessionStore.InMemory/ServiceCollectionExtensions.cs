using ACommerce.Authentication.TwoFactor.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Authentication.TwoFactor.SessionStore.InMemory;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds in-memory two-factor session store (Singleton)
    /// WARNING: Use only for development/testing
    /// </summary>
    public static IServiceCollection AddInMemoryTwoFactorSessionStore(
        this IServiceCollection services)
    {
        services.AddSingleton<ITwoFactorSessionStore, InMemoryTwoFactorSessionStore>();
        return services;
    }
}