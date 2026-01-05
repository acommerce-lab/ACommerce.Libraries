using ACommerce.Complaints.Entities;
using ACommerce.Complaints.Services;
using ACommerce.SharedKernel.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Complaints.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddComplaintsModule(this IServiceCollection services)
    {
        services.AddScoped<IComplaintsService, ComplaintsService>();

        return services;
    }
}
