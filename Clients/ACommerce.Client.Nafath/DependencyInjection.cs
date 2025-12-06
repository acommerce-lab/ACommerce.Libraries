using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.Nafath;

public static class DependencyInjection
{
    /// <summary>
    /// إضافة NafathClient للتطبيق
    /// </summary>
    public static IServiceCollection AddNafathClient(
        this IServiceCollection services,
        string serviceName = "Marketplace")
    {
        services.AddScoped<NafathClient>();

        return services;
    }
}
