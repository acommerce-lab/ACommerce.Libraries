using ACommerce.Templates.Customer.Themes;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Templates.Customer.Services;

/// <summary>
/// Extension methods for registering ACommerce Customer Template services
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Adds ACommerce Customer Template services to the service collection
    /// </summary>
    public static IServiceCollection AddACommerceCustomerTemplate(
        this IServiceCollection services,
        Action<ThemeOptions>? configureTheme = null)
    {
        // Configure theme options
        var themeOptions = new ThemeOptions();
        configureTheme?.Invoke(themeOptions);

        // Register ThemeService as singleton
        services.AddSingleton(sp => new ThemeService(themeOptions));

        return services;
    }

    /// <summary>
    /// Adds ACommerce Customer Template services with custom theme
    /// </summary>
    public static IServiceCollection AddACommerceCustomerTemplate(
        this IServiceCollection services,
        ThemeOptions themeOptions)
    {
        services.AddSingleton(sp => new ThemeService(themeOptions));
        return services;
    }
}
