using Microsoft.Extensions.Logging;
using Order.Shared.Services;

namespace com.order.app;

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
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // API Settings
        var apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5100" // Android Emulator
            : "http://localhost:5100"; // iOS Simulator / Desktop

        // HttpClient for API
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl)
        });

        // API Service
        builder.Services.AddScoped<OrderApiService>();

        // Local Storage & State Services
        builder.Services.AddScoped<LocalStorageService>();
        builder.Services.AddScoped<AuthStateService>();
        builder.Services.AddScoped<AppSettingsService>();

        return builder.Build();
    }
}
