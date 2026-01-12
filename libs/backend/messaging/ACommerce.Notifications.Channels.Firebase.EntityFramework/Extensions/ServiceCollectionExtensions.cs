using ACommerce.Notifications.Channels.Firebase.Storage;
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
    /// يستخدم DbContext المسجل مسبقاً في DI
    /// </summary>
    public static IServiceCollection AddFirebaseTokenStoreEntityFramework(
        this IServiceCollection services)
    {
        // Remove any existing IFirebaseTokenStore registration (like InMemory)
        var existingDescriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IFirebaseTokenStore));

        if (existingDescriptor != null)
        {
            services.Remove(existingDescriptor);
        }

        // Register EF implementation - uses DbContext from DI
        services.AddScoped<IFirebaseTokenStore, EfFirebaseTokenStore>();

        return services;
    }
}
