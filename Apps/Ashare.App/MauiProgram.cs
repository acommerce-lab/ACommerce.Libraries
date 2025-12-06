using Ashare.Shared.Services;
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
using ACommerce.ServiceRegistry.Client.Extensions;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Themes;
using Microsoft.Extensions.Logging;
using ThemeService = Ashare.Shared.Services.ThemeService;
using Ashare.App.Services;

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
        // API Configuration (Centralized in ApiSettings.cs)
        // ═══════════════════════════════════════════════════════════════════
        var apiBaseUrl = ApiSettings.BaseUrl;

        // 🔍 Debug: Log API configuration
        ApiSettings.LogConfiguration();
        Console.WriteLine($"[MauiProgram] 🌐 API Base URL: {apiBaseUrl}");

        // ═══════════════════════════════════════════════════════════════════
        // Client SDKs with Service Discovery (Predefined Services)
        // ═══════════════════════════════════════════════════════════════════

        // ACommerce Client مع خدمات محددة مسبقاً
        // يسجل الخدمات في Cache محلي لاستخدامها من قبل DynamicHttpClient
        builder.Services.AddACommerceClientWithServices(
            services =>
            {
                // تسجيل خدمة Marketplace - تستخدمها معظم الـ Clients
                services.AddService("Marketplace", apiBaseUrl);

                // يمكن إضافة خدمات أخرى إذا كانت على URLs مختلفة
                // services.AddService("Files", "https://files.ashare.app");
                // services.AddService("Payments", "https://payments.ashare.app");
            },
            options =>
            {
                options.TimeoutSeconds = 30;
#if DEBUG
                // تجاوز التحقق من SSL في التطوير (للشهادات الذاتية)
                options.BypassSslValidation = true;
#endif
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

        // Storage Service (MAUI implementation using SecureStorage)
        builder.Services.AddSingleton<IStorageService, Ashare.App.Services.MauiStorageService>();

        // Localization (AR, EN, UR)
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

        // Theme Service (Dark/Light Mode)
        builder.Services.AddSingleton<ThemeService>();

        // Navigation Service (MAUI implementation)
        builder.Services.AddScoped<IAppNavigationService, Ashare.App.Services.AppNavigationService>();

        // Space Data Service (mock data for development)
        builder.Services.AddSingleton<SpaceDataService>();

        // Timezone Service (Device implementation using local timezone)
        builder.Services.AddSingleton<ITimezoneService, DeviceTimezoneService>();

        // ═══════════════════════════════════════════════════════════════════
        // Ashare API Service (ربط التطبيق بالباك اند)
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddScoped<AshareApiService>();

        // ⬅️ إذا لم تُسجَّل بعد:
        builder.Services.AddScoped<CategoriesClient>();          // من ACommerce SDK
        builder.Services.AddScoped<CategoryAttributesClient>(); // للخصائص الديناميكية
        builder.Services.AddScoped<ProductsClient>();            // من ACommerce SDK
        builder.Services.AddScoped<ProductListingsClient>();     // من ACommerce SDK
        builder.Services.AddScoped<OrdersClient>();              // من ACommerce SDK

        // HttpClient Factory باسم "AshareApi" (using centralized ApiSettings)
        builder.Services.AddHttpClient("AshareApi", client =>
        {
            client.BaseAddress = ApiSettings.BaseUri;
        })
#if DEBUG
        // في التطوير: تجاوز التحقق من شهادة SSL الذاتية
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        })
#endif
        ;

        var app = builder.Build();

        // تهيئة Service Cache بالخدمات المحددة مسبقاً
        app.Services.InitializeServiceCache();

        return app;
    }
}
