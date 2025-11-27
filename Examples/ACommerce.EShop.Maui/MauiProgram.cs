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

        // TODO: Client SDK integration requires DynamicHttpClient and ServiceRegistry setup
        // For now, MAUI app should use direct HTTP calls or implement simplified clients
        // The ACommerce.Client.* SDKs are designed for microservices with service discovery
        // See ACommerce.Client.Core.Extensions.AddACommerceClient() for proper setup

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
