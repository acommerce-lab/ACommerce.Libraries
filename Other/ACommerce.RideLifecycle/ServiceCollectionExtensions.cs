using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.RideLifecycle
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryRideOrchestrator(this IServiceCollection services)
        {
            services.AddSingleton<IRideOrchestrator, InMemoryRideOrchestrator>();
            return services;
        }

        public static IServiceCollection AddPersistentRideOrchestrator(this IServiceCollection services)
        {
            // Register only the orchestrator. Host applications must register
            // the RideDbContext themselves (for example: AddDbContext<RideDbContext>(opts => opts.UseSqlite(...))).
            services.AddScoped<IRideOrchestrator, Persistence.PersistentRideOrchestrator>();
            return services;
        }
    }
}
