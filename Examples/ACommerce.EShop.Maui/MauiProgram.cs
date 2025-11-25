using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Syncfusion.Blazor;

namespace ACommerce.EShop.Maui;

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

        // Add Blazor WebView
        builder.Services.AddMauiBlazorWebView();

        // Syncfusion License (add your license key here)
        // Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");

        // Syncfusion Blazor Services
        builder.Services.AddSyncfusionBlazor();

        // Add HTTP Client for API calls
        builder.Services.AddHttpClient("EShopAPI", client =>
        {
            client.BaseAddress = new Uri("https://localhost:5001/api/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register Client SDKs
        builder.Services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("EShopAPI");
            return new ACommerce.Client.Profiles.ProfilesClient(httpClient);
        });

        builder.Services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("EShopAPI");
            return new ACommerce.Client.Catalog.Products.ProductsClient(httpClient);
        });

        builder.Services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("EShopAPI");
            return new ACommerce.Client.Catalog.Attributes.AttributesClient(httpClient);
        });

        builder.Services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("EShopAPI");
            return new ACommerce.Client.Catalog.Units.UnitsClient(httpClient);
        });

        builder.Services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("EShopAPI");
            return new ACommerce.Client.Catalog.Currencies.CurrenciesClient(httpClient);
        });

        builder.Services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("EShopAPI");
            return new ACommerce.Client.Carts.CartsClient(httpClient);
        });

        builder.Services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("EShopAPI");
            return new ACommerce.Client.Orders.OrdersClient(httpClient);
        });

        builder.Services.AddScoped(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("EShopAPI");
            return new ACommerce.Client.Vendors.VendorsClient(httpClient);
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
