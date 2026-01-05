using HamtramckHardware.Shared.Extensions;
using HamtramckHardware.Shared.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Core.Storage;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Services.Analytics;
using ACommerce.Templates.Customer.Themes;
using ACommerce.ServiceRegistry.Client.Extensions;
using Microsoft.Extensions.Logging;
using HamtramckHardware.App.Services;
using ThemeService = HamtramckHardware.Shared.Services.ThemeService;

namespace HamtramckHardware.App;

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

        // Hamtramck Hardware Theme - Industrial Orange
        builder.Services.AddACommerceCustomerTemplate(options =>
        {
            options.Colors.Primary = "#E65100";      // Deep Orange
            options.Colors.PrimaryDark = "#BF360C";
            options.Colors.PrimaryLight = "#FF6D00";
            options.Colors.Secondary = "#455A64";    // Steel Gray
            options.Colors.Success = "#4CAF50";
            options.Colors.Error = "#F44336";
            options.Colors.Warning = "#FF9800";
            options.Colors.Info = "#2196F3";
            options.Colors.Background = "#FAFAFA";
            options.Colors.Surface = "#FFFFFF";
            options.Direction = TextDirection.LTR;
            options.Mode = ThemeMode.Light;
        });

        var apiBaseUrl = ApiSettings.BaseUrl;
        ApiSettings.LogConfiguration();
        Console.WriteLine($"[MauiProgram] API Base URL: {apiBaseUrl}");

        builder.Services.AddSingleton<IStorageService, MauiStorageService>();
        builder.Services.AddSingleton<ITokenStorage, StorageBackedTokenStorage>();
        builder.Services.AddSingleton<TokenManager>();

        builder.Services.AddHamtramckClients(apiBaseUrl, options =>
        {
            options.TokenProvider = sp => sp.GetRequiredService<TokenManager>();
#if DEBUG
            options.BypassSslValidation = true;
#endif
        });

        builder.Services.AddHamtramckServices();

        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddScoped<IAppNavigationService, AppNavigationService>();
        builder.Services.AddSingleton<IPaymentService, MockPaymentService>();
        builder.Services.AddSingleton<IAppVersionService, AppVersionService>();

        builder.Services.AddMockAnalytics();

        builder.Services.AddHttpClient("HamtramckApi", client =>
        {
            client.BaseAddress = ApiSettings.BaseUri;
        })
#if DEBUG
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        })
#endif
        ;

        var app = builder.Build();
        app.Services.InitializeServiceCache();

        return app;
    }
}
