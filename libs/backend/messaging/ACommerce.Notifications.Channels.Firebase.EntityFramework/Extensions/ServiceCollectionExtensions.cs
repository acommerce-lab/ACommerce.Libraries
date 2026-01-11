using ACommerce.Notifications.Channels.Firebase.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Notifications.Channels.Firebase.EntityFramework.Extensions;

/// <summary>
/// Extension methods for registering Firebase EF token store
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// تسجيل EF Firebase Token Store
    /// يخزن Device Tokens في قاعدة البيانات
    /// </summary>
    /// <typeparam name="TContext">نوع DbContext</typeparam>
    /// <param name="services">مجموعة الخدمات</param>
    /// <returns>مجموعة الخدمات</returns>
    public static IServiceCollection AddFirebaseTokenStoreEntityFramework<TContext>(
        this IServiceCollection services)
        where TContext : DbContext
    {
        // Remove any existing IFirebaseTokenStore registration (like InMemory)
        var existingDescriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IFirebaseTokenStore));

        if (existingDescriptor != null)
        {
            services.Remove(existingDescriptor);
        }

        // Register EF implementation
        services.AddScoped<IFirebaseTokenStore, EfFirebaseTokenStore<TContext>>();

        return services;
    }
}
