using ACommerce.LegalPages.Contracts;
using ACommerce.LegalPages.Entities;
using ACommerce.LegalPages.Services;
using ACommerce.SharedKernel.Abstractions.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.LegalPages.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLegalPages(this IServiceCollection services)
    {
        EntityDiscoveryRegistry.RegisterEntity<LegalPage>();
        services.AddScoped<ILegalPagesService, LegalPagesService>();
        return services;
    }
}
