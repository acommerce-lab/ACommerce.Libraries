using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ACommerce.Client.Auth;
using ACommerce.Client.Nafath;
using ACommerce.Client.Bookings;
using ACommerce.Client.Cart.Extensions;
using ACommerce.Client.Categories;
using ACommerce.Client.Categories.Extensions;
using ACommerce.Client.Chats;
using ACommerce.Client.ContactPoints.Extensions;
using ACommerce.Client.Core.Extensions;
using ACommerce.Client.Files;
using ACommerce.Client.Locations.Extensions;
using ACommerce.Client.Notifications;
using ACommerce.Client.Orders;
using ACommerce.Client.Orders.Extensions;
using ACommerce.Client.ProductListings;
using ACommerce.Client.Products;
using ACommerce.Client.Products.Extensions;
using ACommerce.Client.Realtime;
using ACommerce.Client.Vendors;
using ACommerce.Client.Profiles;
using ACommerce.Client.Payments;
using ACommerce.Client.Subscriptions;
using ACommerce.Client.LegalPages;
using ACommerce.Client.Versions;
using ACommerce.ServiceRegistry.Client.Extensions;
using Ashare.Shared.Services;
using ACommerce.Templates.Customer.Services;

namespace Ashare.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAshareClients(
        this IServiceCollection services, 
        string apiBaseUrl,
        Action<ClientOptions>? configureClient = null)
    {
        services.AddACommerceClientWithServices(
            svc =>
            {
                svc.AddService("Marketplace", apiBaseUrl);
                svc.AddService("Ashare", apiBaseUrl);
                svc.AddService("Payments", apiBaseUrl);
                svc.AddService("Files", apiBaseUrl);
            },
            options =>
            {
                options.TimeoutSeconds = 120;
                options.EnableAuthentication = true;
                configureClient?.Invoke(options);
            });

        services.AddScoped<AuthClient>();
        services.AddNafathClient();
        services.AddLocationsClient(apiBaseUrl);
        services.AddProductsClient(apiBaseUrl);
        services.AddCategoriesClient(apiBaseUrl);
        services.AddScoped<ProductListingsClient>();
        services.AddOrdersClient(apiBaseUrl);
        services.AddCartClient(apiBaseUrl);
        services.AddScoped<VendorsClient>();
        services.AddScoped<ProfilesClient>();
        services.AddScoped<SubscriptionClient>();
        services.AddScoped<PaymentsClient>();
        services.AddScoped<ChatsClient>();
        services.AddScoped<NotificationsClient>();
        services.AddContactPointsClient(apiBaseUrl);
        services.AddSingleton<RealtimeClient>();
        services.AddScoped<FilesClient>();
        services.AddScoped<CategoriesClient>();
        services.AddScoped<CategoryAttributesClient>();
        services.AddScoped<ProductsClient>();
        services.AddScoped<ProductListingsClient>();
        services.AddScoped<OrdersClient>();
        services.AddScoped<BookingsClient>();
        services.AddScoped<LegalPagesClient>();
        services.AddScoped<VersionsClient>();

        return services;
    }

    public static IServiceCollection AddAshareServices(this IServiceCollection services)
    {
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<AshareApiService>();
        services.AddScoped<PendingListingService>();
        services.AddScoped<Services.VersionCheckService>();

        return services;
    }
}
