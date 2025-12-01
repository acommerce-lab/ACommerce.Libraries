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
        // ACommerce Customer Template - Ashare Theme (Visual Identity 2025)
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddACommerceCustomerTemplate(options =>
        {
            // Ashare brand colors from Visual Identity Guidelines
            // Primary: Deep Olive Green - symbolizes sustainability, trust, stability
            options.Colors.Primary = "#345454";      // Deep Olive Green
            options.Colors.PrimaryDark = "#263F3F";  // Darker shade
            options.Colors.PrimaryLight = "#4A6B6B"; // Lighter shade

            // Secondary: Soft Orange - represents vitality, optimism, positive energy
            options.Colors.Secondary = "#F4844C";    // Soft Orange

            // Supporting colors
            options.Colors.Success = "#10B981";      // Emerald-500
            options.Colors.Error = "#EF4444";        // Red-500
            options.Colors.Warning = "#F59E0B";      // Amber-500
            options.Colors.Info = "#345454";         // Use primary for info

            // Background: Light peach/cream from brand guidelines
            options.Colors.Background = "#FEE8D6";   // Light cream background
            options.Colors.Surface = "#FFFFFF";      // White surface

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
        if (DeviceInfo.Platform == DevicePlatform.Android)
            apiBaseUrl = "https://10.0.2.2:5001";
        else if (DeviceInfo.Platform == DevicePlatform.WinUI)
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
        builder.Services.AddScoped<ChatsClient>();

        // Notifications Client
        builder.Services.AddScoped<NotificationsClient>();

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

        // ═══════════════════════════════════════════════════════════════════
        // Ashare API Service (ربط التطبيق بالباك اند)
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddScoped<AshareApiService>();

        // ⬅️ إذا لم تُسجَّل بعد:
        builder.Services.AddScoped<CategoriesClient>();          // من ACommerce SDK
        builder.Services.AddScoped<ProductsClient>();            // من ACommerce SDK
        builder.Services.AddScoped<ProductListingsClient>();     // من ACommerce SDK
        builder.Services.AddScoped<OrdersClient>();              // من ACommerce SDK

        // HttpClient Factory باسم "AshareApi" كما يستخدم AshareApiService
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            // Android Emulator لا يصل إلى localhost مباشرة
            builder.Services.AddHttpClient("AshareApi", client =>
            {
                client.BaseAddress = new Uri("https://10.0.2.2:5001");
            });
        }
        else if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            // Windows يمكنه الوصول مباشرة
            builder.Services.AddHttpClient("AshareApi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001");
            });
        }
        else
        {
            // Production أو أي منصة أخرى
            builder.Services.AddHttpClient("AshareApi", client =>
            {
                client.BaseAddress = new Uri("https://api.ashare.app");
            });
        }



        return builder.Build();
    }
}
