using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Templates.Customer.Services.Localization;

public static class LocalizationExtensions
{
    public static IServiceCollection AddLocalizationValidation(this IServiceCollection services)
    {
        services.AddScoped<LocalizationValidationService>();
        return services;
    }

    public static IServiceProvider ValidateLocalization(this IServiceProvider serviceProvider, bool isDevelopment)
    {
        using var scope = serviceProvider.CreateScope();
        var validator = scope.ServiceProvider.GetService<LocalizationValidationService>();
        validator?.ValidateOnStartup(isDevelopment);
        return serviceProvider;
    }
}
