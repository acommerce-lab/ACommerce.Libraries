using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.Bookings.Extensions;

/// <summary>
/// Extension methods لتسجيل BookingsClient في DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// تسجيل BookingsClient في الخدمات
    /// </summary>
    public static IServiceCollection AddBookingsClient(this IServiceCollection services)
    {
        services.AddScoped<BookingsClient>();
        return services;
    }
}
