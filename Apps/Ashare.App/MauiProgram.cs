using Ashare.App.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Auth.Extensions;
using ACommerce.Client.Cart;
using ACommerce.Client.Cart.Extensions;
using ACommerce.Client.Categories;
using ACommerce.Client.Categories.Extensions;
using ACommerce.Client.Chats;
using ACommerce.Client.ContactPoints;
using ACommerce.Client.ContactPoints.Extensions;
using ACommerce.Client.Core;
using ACommerce.Client.Core.Extensions;
using ACommerce.Client.Files;
using ACommerce.Client.Locations;
using ACommerce.Client.Locations.Extensions;
using ACommerce.Client.Notifications;
using ACommerce.Client.Orders;
using ACommerce.Client.Orders.Extensions;
using ACommerce.Client.ProductListings;
using ACommerce.Client.Products;
using ACommerce.Client.Products.Extensions;
using ACommerce.Client.Profiles;
using ACommerce.Client.Realtime;
using ACommerce.Client.Vendors;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Themes;
using Microsoft.Extensions.Logging;
using ThemeService = Ashare.App.Services.ThemeService;

namespace Ashare.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // ═══════════════════════════════════════════════════════════════════
        // ACommerce Customer Template - Ashare Theme
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddACommerceCustomerTemplate(options =>
        {
            // Ashare brand colors - Sky Blue theme
            options.Colors.Primary = "#0EA5E9";      // Sky-500
            options.Colors.PrimaryDark = "#0284C7";  // Sky-600
            options.Colors.PrimaryLight = "#38BDF8"; // Sky-400
            options.Colors.Secondary = "#8B5CF6";    // Violet-500
            options.Colors.Success = "#10B981";      // Emerald-500
            options.Colors.Error = "#EF4444";        // Red-500
            options.Colors.Warning = "#F59E0B";      // Amber-500
            options.Colors.Info = "#06B6D4";         // Cyan-500

            // Set text direction for Arabic
            options.Direction = TextDirection.RTL;

            // Start with light mode
            options.Mode = ThemeMode.Light;
        });

        // ═══════════════════════════════════════════════════════════════════
        // API Configuration
        // ═══════════════════════════════════════════════════════════════════
        var apiBaseUrl = "https://api.ashare.app";
#if DEBUG
        apiBaseUrl = "https://localhost:5001";
#endif

        // ═══════════════════════════════════════════════════════════════════
        // Client SDKs
        // ═══════════════════════════════════════════════════════════════════

        // Core HTTP Client with interceptors
        builder.Services.AddACommerceClient(apiBaseUrl, options =>
        {
            options.TimeoutSeconds = 30;
        });

        // Authentication Client (JWT + Nafath)
        builder.Services.AddAuthClient(apiBaseUrl);

        // Profiles Client
        //builder.Services.AddProfilesClient(options =>
        //{
        //    options.BaseUrl = apiBaseUrl;
        //});

        // Locations Client
        builder.Services.AddLocationsClient(apiBaseUrl);

        // ═══════════════════════════════════════════════════════════════════
        // Catalog Clients (Spaces = Products with Dynamic Attributes)
        // ═══════════════════════════════════════════════════════════════════

        // Products Client (Spaces)
        builder.Services.AddProductsClient(apiBaseUrl);

        // Categories Client (Space Types)
        builder.Services.AddCategoriesClient(apiBaseUrl);

        // ProductListings Client (Owner Listings)
        builder.Services.AddScoped<ProductListingsClient>();

        // ═══════════════════════════════════════════════════════════════════
        // Sales Clients (Bookings = Orders)
        // ═══════════════════════════════════════════════════════════════════

        // Orders Client (Bookings)
        builder.Services.AddOrdersClient(apiBaseUrl);

        // Cart Client
        builder.Services.AddCartClient(apiBaseUrl);

        // ═══════════════════════════════════════════════════════════════════
        // Marketplace Clients (Hosts = Vendors)
        // ═══════════════════════════════════════════════════════════════════

        // Vendors Client (Hosts)
        builder.Services.AddScoped<VendorsClient>();

        // ═══════════════════════════════════════════════════════════════════
        // Communication Clients
        // ═══════════════════════════════════════════════════════════════════

        // Chats Client
        //builder.Services.AddChatsClient(options =>
        //{
        //    options.BaseUrl = apiBaseUrl;
        //});

        // Notifications Client
        //builder.Services.AddNotificationsClient(options =>
        //{
        //    options.BaseUrl = apiBaseUrl;
        //});

        // ContactPoints Client
        builder.Services.AddContactPointsClient(apiBaseUrl);

        // Real-time Client (SignalR)
        builder.Services.AddSingleton<RealtimeClient>();

        // Files Client
        builder.Services.AddScoped<FilesClient>();

        // ═══════════════════════════════════════════════════════════════════
        // App Services
        // ═══════════════════════════════════════════════════════════════════

        // Localization (AR, EN, UR)
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

        // Theme Service (Dark/Light Mode)
        builder.Services.AddSingleton<ThemeService>();

        // Navigation Service
        builder.Services.AddScoped<IAppNavigationService, AppNavigationService>();

        // Space Data Service (mock data for development)
        builder.Services.AddSingleton<SpaceDataService>();

        return builder.Build();
    }
}
