using Ashare.Shared.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Auth.Extensions;
using ACommerce.Client.Nafath;
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
using ACommerce.Client.Subscriptions;
using ACommerce.Client.Vendors;
using ACommerce.Client.Payments;
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

        // Storage Service (MAUI implementation using SecureStorage) - يجب تسجيله قبل TokenManager
        builder.Services.AddSingleton<IStorageService, Ashare.App.Services.MauiStorageService>();

        // Token Storage (يستخدم IStorageService للتخزين الدائم)
        builder.Services.AddSingleton<ITokenStorage, TokenStorageService>();

        // Token Manager (singleton) - يستخدم ITokenStorage للحفاظ على الجلسة
        builder.Services.AddSingleton<TokenManager>();

        // ACommerce Client مع خدمات محددة مسبقاً
        // يسجل الخدمات في Cache محلي لاستخدامها من قبل DynamicHttpClient
        builder.Services.AddACommerceClientWithServices(
            services =>
            {
                // تسجيل خدمة Marketplace - تستخدمها معظم الـ Clients
                services.AddService("Marketplace", apiBaseUrl);

                // تسجيل خدمة Ashare - للـ SignalR و الإشعارات
                services.AddService("Ashare", apiBaseUrl);

                // تسجيل خدمة Payments - للدفع عبر Noon وغيرها
                services.AddService("Payments", apiBaseUrl);

                // يمكن إضافة خدمات أخرى إذا كانت على URLs مختلفة
                // services.AddService("Files", "https://files.ashare.app");
                // services.AddService("Payments", "https://payments.ashare.app");
            },
            options =>
            {
                options.TimeoutSeconds = 30;
                // تفعيل Authentication لإرسال التوكن مع كل طلب
                options.EnableAuthentication = true;
                options.TokenProvider = sp => sp.GetRequiredService<TokenManager>();
#if DEBUG
                // تجاوز التحقق من SSL في التطوير (للشهادات الذاتية)
                options.BypassSslValidation = true;
#endif
            });

        // Authentication Client (يستخدم TokenManager المسجل أعلاه)
        builder.Services.AddScoped<AuthClient>();

        // Nafath Client (مصادقة نفاذ)
        builder.Services.AddNafathClient();

        // Profiles Client
        builder.Services.AddScoped<ProfilesClient>();

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

        // Subscriptions Client (Host/Vendor Subscription Plans)
        builder.Services.AddScoped<SubscriptionClient>();

        // Payments Client (Payment Gateway Integration)
        builder.Services.AddScoped<PaymentsClient>();

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
#if DEBUG
        // تجاوز SSL في التطوير للشهادات الذاتية
        builder.Services.Configure<RealtimeClientOptions>(options =>
        {
            options.BypassSslValidation = true;
        });
#endif
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

        // Guest Mode Service (allows browsing without login)
        builder.Services.AddSingleton<ACommerce.Templates.Customer.Services.GuestModeService>();

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
        builder.Services.AddScoped<PendingListingService>();

        // ═══════════════════════════════════════════════════════════════════
        // Payment Service (خدمة الدفع)
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddSingleton<IPaymentService, MauiPaymentService>();

        // ═══════════════════════════════════════════════════════════════════
        // Analytics Services (Meta, Google, TikTok, Snapchat)
        // ضع المعرفات في AnalyticsSettings.cs
        // لاختبار الأحداث محلياً: UseMockProvider = true في AnalyticsSettings.cs
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.Configure<ACommerce.Templates.Customer.Services.Analytics.AnalyticsOptions>(options =>
        {
            options.Enabled = AnalyticsSettings.IsEnabled || AnalyticsSettings.UseMockProvider;
            options.Meta = new ACommerce.Templates.Customer.Services.Analytics.AnalyticsConfig
            {
                AppId = AnalyticsSettings.UseMockProvider ? "MOCK_META" : AnalyticsSettings.GetMetaAppId(),
                IosAppId = AnalyticsSettings.MetaIosAppId,
                AndroidAppId = AnalyticsSettings.MetaAndroidAppId,
                DebugMode = AnalyticsSettings.DebugMode
            };
            options.Google = new ACommerce.Templates.Customer.Services.Analytics.AnalyticsConfig
            {
                AppId = AnalyticsSettings.UseMockProvider ? "MOCK_GOOGLE" : AnalyticsSettings.GetGoogleAppId(),
                IosAppId = AnalyticsSettings.FirebaseIosAppId,
                AndroidAppId = AnalyticsSettings.FirebaseAndroidAppId,
                DebugMode = AnalyticsSettings.DebugMode
            };
            options.TikTok = new ACommerce.Templates.Customer.Services.Analytics.AnalyticsConfig
            {
                AppId = AnalyticsSettings.UseMockProvider ? "MOCK_TIKTOK" : AnalyticsSettings.GetTikTokAppId(),
                IosAppId = AnalyticsSettings.TikTokIosAppId,
                AndroidAppId = AnalyticsSettings.TikTokAndroidAppId,
                DebugMode = AnalyticsSettings.DebugMode
            };
            options.Snapchat = new ACommerce.Templates.Customer.Services.Analytics.AnalyticsConfig
            {
                AppId = AnalyticsSettings.UseMockProvider ? "MOCK_SNAPCHAT" : AnalyticsSettings.GetSnapchatAppId(),
                IosAppId = AnalyticsSettings.SnapchatIosAppId,
                AndroidAppId = AnalyticsSettings.SnapchatAndroidAppId,
                DebugMode = AnalyticsSettings.DebugMode
            };
        });
        
        // تسجيل MockAnalyticsProvider للاختبار المحلي
        builder.Services.AddScoped<ACommerce.Templates.Customer.Services.Analytics.MockAnalyticsProvider>();
        builder.Services.AddScoped<ACommerce.Templates.Customer.Services.Analytics.Providers.MetaAnalyticsProvider>();
        builder.Services.AddScoped<ACommerce.Templates.Customer.Services.Analytics.Providers.GoogleAnalyticsProvider>();
        builder.Services.AddScoped<ACommerce.Templates.Customer.Services.Analytics.Providers.TikTokAnalyticsProvider>();
        builder.Services.AddScoped<ACommerce.Templates.Customer.Services.Analytics.Providers.SnapchatAnalyticsProvider>();
        builder.Services.AddScoped<ACommerce.Templates.Customer.Services.Analytics.AnalyticsService>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ACommerce.Templates.Customer.Services.Analytics.AnalyticsOptions>>();
            var service = new ACommerce.Templates.Customer.Services.Analytics.AnalyticsService(options);
            
            if (AnalyticsSettings.UseMockProvider)
            {
                // وضع الاختبار: استخدام MockAnalyticsProvider فقط
                Console.WriteLine("🧪 [Analytics] Mock Mode ENABLED - جميع الأحداث ستظهر في Console");
                service.AddProvider(sp.GetRequiredService<ACommerce.Templates.Customer.Services.Analytics.MockAnalyticsProvider>());
            }
            else
            {
                // وضع الإنتاج: استخدام جميع المنصات
                service.AddProvider(sp.GetRequiredService<ACommerce.Templates.Customer.Services.Analytics.Providers.MetaAnalyticsProvider>());
                service.AddProvider(sp.GetRequiredService<ACommerce.Templates.Customer.Services.Analytics.Providers.GoogleAnalyticsProvider>());
                service.AddProvider(sp.GetRequiredService<ACommerce.Templates.Customer.Services.Analytics.Providers.TikTokAnalyticsProvider>());
                service.AddProvider(sp.GetRequiredService<ACommerce.Templates.Customer.Services.Analytics.Providers.SnapchatAnalyticsProvider>());
            }
            
            return service;
        });

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
