using Ashare.Shared.Extensions;
using Ashare.Shared.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Realtime;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Services.Analytics;
using ACommerce.Templates.Customer.Themes;
using ACommerce.ServiceRegistry.Client.Extensions;
using Microsoft.Extensions.Logging;
using Ashare.App.Services;
using ThemeService = Ashare.Shared.Services.ThemeService;

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

        builder.Services.AddACommerceCustomerTemplate(options =>
        {
            options.Colors.Primary = "#345454";
            options.Colors.PrimaryDark = "#263F3F";
            options.Colors.PrimaryLight = "#4A6B6B";
            options.Colors.Secondary = "#F4844C";
            options.Colors.Success = "#10B981";
            options.Colors.Error = "#EF4444";
            options.Colors.Warning = "#F59E0B";
            options.Colors.Info = "#345454";
            options.Colors.Background = "#FEE8D6";
            options.Colors.Surface = "#FFFFFF";
            options.Direction = TextDirection.RTL;
            options.Mode = ThemeMode.Light;
        });

        var apiBaseUrl = ApiSettings.BaseUrl;
        ApiSettings.LogConfiguration();
        Console.WriteLine($"[MauiProgram] 🌐 API Base URL: {apiBaseUrl}");

        builder.Services.AddSingleton<IStorageService, MauiStorageService>();
        builder.Services.AddSingleton<ITokenStorage, TokenStorageService>();
        builder.Services.AddSingleton<TokenManager>();

        builder.Services.AddAshareClients(apiBaseUrl, options =>
        {
            options.TokenProvider = sp => sp.GetRequiredService<TokenManager>();
#if DEBUG
            options.BypassSslValidation = true;
#endif
        });

#if DEBUG
        builder.Services.Configure<RealtimeClientOptions>(options =>
        {
            options.BypassSslValidation = true;
        });
#endif

        builder.Services.AddAshareServices();
        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddSingleton<GuestModeService>();
        builder.Services.AddScoped<IAppNavigationService, AppNavigationService>();
        builder.Services.AddSingleton<SpaceDataService>();
        builder.Services.AddSingleton<ITimezoneService, DeviceTimezoneService>();
        builder.Services.AddSingleton<IPaymentService, MauiPaymentService>();

        builder.Services.AddACommerceAnalytics(options =>
        {
            options.Enabled = AnalyticsSettings.IsEnabled || AnalyticsSettings.UseMockProvider;
            options.Meta = new AnalyticsConfig
            {
                AppId = AnalyticsSettings.GetMetaAppId(),
                IosAppId = AnalyticsSettings.MetaIosAppId,
                AndroidAppId = AnalyticsSettings.MetaAndroidAppId,
                DebugMode = AnalyticsSettings.DebugMode
            };
            options.Google = new AnalyticsConfig
            {
                AppId = AnalyticsSettings.GetGoogleAppId(),
                IosAppId = AnalyticsSettings.FirebaseIosAppId,
                AndroidAppId = AnalyticsSettings.FirebaseAndroidAppId,
                DebugMode = AnalyticsSettings.DebugMode
            };
            options.TikTok = new AnalyticsConfig
            {
                AppId = AnalyticsSettings.GetTikTokAppId(),
                IosAppId = AnalyticsSettings.TikTokIosAppId,
                AndroidAppId = AnalyticsSettings.TikTokAndroidAppId,
                DebugMode = AnalyticsSettings.DebugMode
            };
            options.Snapchat = new AnalyticsConfig
            {
                AppId = AnalyticsSettings.GetSnapchatAppId(),
                IosAppId = AnalyticsSettings.SnapchatIosAppId,
                AndroidAppId = AnalyticsSettings.SnapchatAndroidAppId,
                DebugMode = AnalyticsSettings.DebugMode
            };
        }, useMockProvider: AnalyticsSettings.UseMockProvider);

        builder.Services.AddHttpClient("AshareApi", client =>
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
