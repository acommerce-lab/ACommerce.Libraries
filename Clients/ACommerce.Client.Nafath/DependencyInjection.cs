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
        services.AddSingleton(sp =>
        {
            var httpClient = sp.GetRequiredService<ACommerce.Client.Core.Http.IApiClient>();
            return new NafathClient(httpClient, serviceName);
        });

        return services;
    }
}
