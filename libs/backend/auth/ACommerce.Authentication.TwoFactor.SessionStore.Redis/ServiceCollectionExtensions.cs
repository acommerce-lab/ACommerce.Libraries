using ACommerce.Authentication.TwoFactor.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ACommerce.Authentication.TwoFactor.SessionStore.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisTwoFactorSessionStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisSessionStoreOptions>(
            configuration.GetSection(RedisSessionStoreOptions.SectionName));

        var options = configuration
            .GetSection(RedisSessionStoreOptions.SectionName)
            .Get<RedisSessionStoreOptions>() ?? new RedisSessionStoreOptions();

        var connectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
            ?? options.ConnectionString;

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(connectionString));

        services.AddScoped<ITwoFactorSessionStore, RedisTwoFactorSessionStore>();

        return services;
    }

    public static IServiceCollection AddRedisTwoFactorSessionStore(
        this IServiceCollection services,
        string connectionString)
    {
        services.Configure<RedisSessionStoreOptions>(options =>
        {
            options.ConnectionString = connectionString;
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(connectionString));

        services.AddScoped<ITwoFactorSessionStore, RedisTwoFactorSessionStore>();

        return services;
    }
}
