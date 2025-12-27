using Microsoft.Extensions.DependencyInjection;
using ACommerce.Client.Auth;
using ACommerce.Client.Cart.Extensions;
using ACommerce.Client.Categories;
using ACommerce.Client.Categories.Extensions;
using ACommerce.Client.Core.Extensions;
using ACommerce.Client.Core.Interceptors;
using ACommerce.Client.Orders;
using ACommerce.Client.Orders.Extensions;
using ACommerce.Client.Payments;
using ACommerce.Client.Products;
using ACommerce.Client.Products.Extensions;
using ACommerce.Client.Profiles;
using ACommerce.Client.Versions;
using ACommerce.ServiceRegistry.Client.Extensions;
using ACommerce.Templates.Customer.Services;
using HamtramckHardware.Shared.Services;

namespace HamtramckHardware.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHamtramckClients(
        this IServiceCollection services,
        string apiBaseUrl,
        Action<ClientOptions>? configureClient = null)
    {
        services.AddACommerceClientWithServices(
            svc =>
            {
                svc.AddService("Marketplace", apiBaseUrl);
            },
            options =>
            {
                options.TimeoutSeconds = 120;
                options.EnableAuthentication = true;
                configureClient?.Invoke(options);
            });

        // Auth
        services.AddScoped<AuthClient>();

        // Catalog
        services.AddProductsClient(apiBaseUrl);
        services.AddCategoriesClient(apiBaseUrl);
        services.AddScoped<CategoriesClient>();
        services.AddScoped<ProductsClient>();

        // Sales
        services.AddOrdersClient(apiBaseUrl);
        services.AddCartClient(apiBaseUrl);
        services.AddScoped<OrdersClient>();
        services.AddScoped<PaymentsClient>();

        // Profiles
        services.AddScoped<ProfilesClient>();

        // Versions
        services.AddScoped<VersionsClient>();

        return services;
    }

    public static IServiceCollection AddHamtramckServices(this IServiceCollection services)
    {
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddSingleton<ThemeService>();

        return services;
    }
}

public class ClientOptions
{
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableAuthentication { get; set; } = true;
    public bool BypassSslValidation { get; set; } = false;
    public Func<IServiceProvider, ITokenProvider>? TokenProvider { get; set; }
}
