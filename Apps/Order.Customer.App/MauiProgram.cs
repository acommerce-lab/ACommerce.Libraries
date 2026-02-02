using Microsoft.Extensions.Logging;
using ACommerce.Client.Core.Storage;
using ACommerce.Templates.Customer.Services;
using com.order.app.Services;
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

        // Storage Service (MAUI SecureStorage + Preferences)
        builder.Services.AddScoped<IStorageService, MauiStorageService>();

        // Template Services (AuthState, AppSettings, GuestMode)
        builder.Services.AddACommerceCustomerTemplate();

        return builder.Build();
    }
}
