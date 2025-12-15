using ACommerce.App.Customer.Services;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Themes;
using Ashare.Shared.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Auth.Extensions;
using ACommerce.Client.Nafath;
using ACommerce.Client.Cart;
using ACommerce.Client.Cart.Extensions;
using ACommerce.Client.Categories;
using ACommerce.Client.Categories.Extensions;
using ACommerce.Client.Chats;
using ACommerce.Client.Locations;
using ACommerce.Client.Locations.Extensions;
using ACommerce.Client.Notifications;
using ACommerce.Client.Orders;
using ACommerce.Client.Orders.Extensions;
using ACommerce.Client.ProductListings;
using ACommerce.Client.Products;
using ACommerce.Client.Products.Extensions;
using ACommerce.Client.Vendors;
using ACommerce.Client.Profiles;
using ACommerce.Client.Payments;
using ACommerce.Client.Subscriptions;
using ACommerce.Client.Files;
using ACommerce.Client.Realtime;
using ACommerce.Client.Core.Extensions;
using ACommerce.ServiceRegistry.Client.Extensions;
using Microsoft.Extensions.Logging;

namespace ACommerce.App.Customer;

public static class MauiProgram
{
    // API Base URL - يمكن تغييره حسب البيئة
    private const string ApiBaseUrl = "https://ashare-api-130415035604.me-central2.run.app";

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
        // Add ACommerce Customer Template with Ashare Visual Identity 2025
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddACommerceCustomerTemplate(options =>
        {
            // Ashare Visual Identity 2025 Colors
            // Primary: Royal Blue - elegant, professional, trustworthy
            options.Colors.Primary = "#1E40AF";
            // Secondary: Soft Orange - vitality, optimism, positive energy
            options.Colors.Secondary = "#F4844C";
            options.Colors.Success = "#10B981";
            options.Colors.Error = "#EF4444";
            options.Colors.Warning = "#F59E0B";
            options.Colors.Info = "#1E40AF";

            // Set text direction for Arabic
            options.Direction = TextDirection.RTL;

            // Start with light mode
            options.Mode = ThemeMode.Light;
        });

        // ═══════════════════════════════════════════════════════════════════
        // Storage & Token Management
        // ═══════════════════════════════════════════════════════════════════
        
        // Storage Service (MAUI Preferences implementation)
        builder.Services.AddSingleton<IStorageService, MauiStorageService>();
        
        // Token Storage
        builder.Services.AddSingleton<ITokenStorage, MauiTokenStorage>();
        
        // Token Manager
        builder.Services.AddSingleton<TokenManager>();

        // ═══════════════════════════════════════════════════════════════════
        // Client SDKs with Service Discovery
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddACommerceClientWithServices(
            services =>
            {
                services.AddService("Marketplace", ApiBaseUrl);
                services.AddService("Ashare", ApiBaseUrl);
                services.AddService("Payments", ApiBaseUrl);
                services.AddService("Files", ApiBaseUrl);
            },
            options =>
            {
                options.TimeoutSeconds = 120;
                options.EnableAuthentication = true;
            });

        // ═══════════════════════════════════════════════════════════════════
        // Authentication Clients
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddScoped<AuthClient>();
        builder.Services.AddNafathClient();

        // ═══════════════════════════════════════════════════════════════════
        // Catalog Clients (Spaces = Products)
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddProductsClient(ApiBaseUrl);
        builder.Services.AddCategoriesClient(ApiBaseUrl);
        builder.Services.AddScoped<ProductListingsClient>();

        // ═══════════════════════════════════════════════════════════════════
        // Sales Clients (Bookings = Orders)
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddOrdersClient(ApiBaseUrl);
        builder.Services.AddCartClient(ApiBaseUrl);

        // ═══════════════════════════════════════════════════════════════════
        // Marketplace Clients (Hosts = Vendors)
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddScoped<VendorsClient>();
        builder.Services.AddScoped<ProfilesClient>();
        builder.Services.AddScoped<SubscriptionClient>();
        builder.Services.AddScoped<PaymentsClient>();

        // ═══════════════════════════════════════════════════════════════════
        // Communication Clients
        // ═══════════════════════════════════════════════════════════════════
        builder.Services.AddScoped<ChatsClient>();
        builder.Services.AddScoped<NotificationsClient>();
        builder.Services.AddLocationsClient(ApiBaseUrl);
        builder.Services.AddSingleton<RealtimeClient>();
        builder.Services.AddScoped<FilesClient>();

        // ═══════════════════════════════════════════════════════════════════
        // App Services
        // ═══════════════════════════════════════════════════════════════════
        
        // Localization (AR, EN, UR) - استخدام الترجمة من Ashare.Shared
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
        
        // Navigation Service
        builder.Services.AddScoped<IAppNavigationService, AppNavigationService>();
        
        // Mock Data Service (للتطوير فقط - سيتم استبداله بـ API)
        builder.Services.AddSingleton<MockDataService>();

        return builder.Build();
    }
}

// ═══════════════════════════════════════════════════════════════════
// MAUI Storage Service Implementation
// ═══════════════════════════════════════════════════════════════════
public class MauiStorageService : IStorageService
{
    public Task<string?> GetAsync(string key)
    {
        var value = Preferences.Default.Get<string>(key, null!);
        return Task.FromResult(value);
    }

    public Task SetAsync(string key, string value)
    {
        Preferences.Default.Set(key, value);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        Preferences.Default.Remove(key);
        return Task.CompletedTask;
    }
}

// ═══════════════════════════════════════════════════════════════════
// MAUI Token Storage Implementation
// ═══════════════════════════════════════════════════════════════════
public class MauiTokenStorage : ITokenStorage
{
    private const string TokenKey = "auth_token";
    private const string ExpiresAtKey = "auth_token_expires_at";

    public Task SaveTokenAsync(string token, DateTime? expiresAt)
    {
        Preferences.Default.Set(TokenKey, token);
        if (expiresAt.HasValue)
        {
            Preferences.Default.Set(ExpiresAtKey, expiresAt.Value.ToString("O"));
        }
        else
        {
            Preferences.Default.Remove(ExpiresAtKey);
        }
        return Task.CompletedTask;
    }

    public Task<(string? Token, DateTime? ExpiresAt)> GetTokenAsync()
    {
        var token = Preferences.Default.Get<string>(TokenKey, null!);
        var expiresAtStr = Preferences.Default.Get<string>(ExpiresAtKey, null!);
        
        DateTime? expiresAt = null;
        if (!string.IsNullOrEmpty(expiresAtStr) && DateTime.TryParse(expiresAtStr, out var parsed))
        {
            expiresAt = parsed;
        }
        
        return Task.FromResult((token, expiresAt));
    }

    public Task<bool> HasTokenAsync()
    {
        var token = Preferences.Default.Get<string>(TokenKey, null!);
        return Task.FromResult(!string.IsNullOrEmpty(token));
    }

    public Task ClearTokenAsync()
    {
        Preferences.Default.Remove(TokenKey);
        Preferences.Default.Remove(ExpiresAtKey);
        return Task.CompletedTask;
    }
}
