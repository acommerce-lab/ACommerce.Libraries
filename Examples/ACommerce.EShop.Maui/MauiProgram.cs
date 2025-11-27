using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Syncfusion.Blazor;
using ACommerce.Client.Core.Extensions;
using ACommerce.Client.Auth;
using ACommerce.Client.Cart;
using ACommerce.Client.Categories;
using ACommerce.Client.Orders;
using ACommerce.Client.ProductListings;
using ACommerce.Client.Products;
using ACommerce.Client.Profiles;
using ACommerce.Client.Vendors;
using ACommerce.Blazor.Shop.Services;

namespace ACommerce.EShop.Maui;

public static class MauiProgram
{
    // API Base URL - change this to match your backend server
    private const string ApiBaseUrl = "http://localhost:5001";

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

        // Add Blazor WebView
        builder.Services.AddMauiBlazorWebView();

        // Syncfusion License (add your license key here)
        // Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");

        // Syncfusion Blazor Services
        builder.Services.AddSyncfusionBlazor();

        // ACommerce Client SDK with Static URL (for standalone MAUI app)
        builder.Services.AddACommerceStaticClient(ApiBaseUrl);

        // Register Client SDKs (they depend on IApiClient which is now registered)
        builder.Services.AddScoped<AuthClient>();
        builder.Services.AddScoped<ProductsClient>();
        builder.Services.AddScoped<ProductListingsClient>();
        builder.Services.AddScoped<CartClient>();
        builder.Services.AddScoped<OrdersClient>();
        builder.Services.AddScoped<ProfilesClient>();
        builder.Services.AddScoped<VendorsClient>();
        builder.Services.AddScoped<CategoriesClient>();

        // App Services (from Blazor.Shop template)
        builder.Services.AddScoped<ThemeService>();
        builder.Services.AddScoped<CartStateService>();
        builder.Services.AddScoped<NotificationService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
