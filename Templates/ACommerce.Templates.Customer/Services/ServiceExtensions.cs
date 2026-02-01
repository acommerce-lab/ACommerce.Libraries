using ACommerce.Client.Core.Storage;
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

        // Register state management services
        services.AddScoped<AuthStateService>();
        services.AddScoped<AppSettingsService>();
        services.AddScoped<GuestModeService>();

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

        // Register state management services
        services.AddScoped<AuthStateService>();
        services.AddScoped<AppSettingsService>();
        services.AddScoped<GuestModeService>();

        return services;
    }

    /// <summary>
    /// Adds storage service (required for AuthStateService and AppSettingsService)
    /// </summary>
    public static IServiceCollection AddACommerceStorage<TStorage>(this IServiceCollection services)
        where TStorage : class, IStorageService
    {
        services.AddScoped<IStorageService, TStorage>();
        return services;
    }

    /// <summary>
    /// Adds in-memory storage (for testing or platforms without persistent storage)
    /// </summary>
    public static IServiceCollection AddACommerceInMemoryStorage(this IServiceCollection services)
    {
        services.AddScoped<IStorageService, InMemoryStorageService>();
        return services;
    }
}
