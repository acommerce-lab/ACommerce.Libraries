using ACommerce.Authentication.TwoFactor.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Entity Framework two-factor session store (Scoped)
    /// </summary>
    public static IServiceCollection AddEntityFrameworkTwoFactorSessionStore<TContext>(
        this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddScoped<ITwoFactorSessionStore, EfTwoFactorSessionStore<TContext>>();
        return services;
    }
}
