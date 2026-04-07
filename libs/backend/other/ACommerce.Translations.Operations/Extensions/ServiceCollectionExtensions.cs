using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.Translations.Operations.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Translations.Operations.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// يسجّل TranslationService ويُسجّل كيانات Translation/Language في الـ EntityDiscoveryRegistry.
    /// </summary>
    public static IServiceCollection AddTranslationOperations(this IServiceCollection services)
    {
        EntityDiscoveryRegistry.RegisterEntity(typeof(Translation));
        EntityDiscoveryRegistry.RegisterEntity(typeof(Language));

        services.AddScoped<TranslationService>();
        return services;
    }
}
