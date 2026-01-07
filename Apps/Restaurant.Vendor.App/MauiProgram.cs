using Microsoft.Extensions.Logging;
using Restaurant.Vendor.App.Services;

namespace Restaurant.Vendor.App;

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

        // API Configuration
        var apiBaseUrl = ApiSettings.BaseUrl;
        Console.WriteLine($"[MauiProgram] API Base URL: {apiBaseUrl}");

        // Register Services
        builder.Services.AddSingleton<ApiSettings>();

        // HttpClient for API calls
        builder.Services.AddHttpClient("RestaurantApi", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Accept-Language", "ar");
        })
#if DEBUG
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        })
#endif
        ;

        // SignalR for real-time order notifications
        builder.Services.AddSingleton<RadarService>();

        var app = builder.Build();
        return app;
    }
}
