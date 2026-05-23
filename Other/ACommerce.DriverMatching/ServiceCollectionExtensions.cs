using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.DriverMatching
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryDriverMatching(this IServiceCollection services)
        {
            services.AddSingleton<IDriverMatchingService, InMemoryDriverMatchingService>();
            return services;
        }
    }
}
