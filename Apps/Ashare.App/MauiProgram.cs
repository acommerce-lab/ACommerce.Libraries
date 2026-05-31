using Ashare.Shared.Extensions;
using Ashare.Shared.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Realtime;
using ACommerce.Client.Core.Storage;
using ACommerce.Client.Files;
using ACommerce.Client.Notifications;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Services.Analytics;
using ACommerce.Templates.Customer.Themes;
using ACommerce.ServiceRegistry.Client.Extensions;
using Microsoft.Extensions.Logging;
using Ashare.App.Services;
using ThemeService = Ashare.Shared.Services.ThemeService;
using Microsoft.Maui.LifecycleEvents;

#if ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif

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
            })
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                // Custom WebView handler for 3DS/OTP payment verification
                handlers.AddHandler<WebView, Ashare.App.Platforms.Android.Handlers.PaymentWebViewHandler>();
#endif
            })
            // Firebase Cloud Messaging initialization (required for v4.0+)
            // Note: iOS initialization is handled in AppDelegate.cs to ensure proper order
            .ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android => android.OnCreate((activity, _) =>
                {
                    CrossFirebase.Initialize(activity, () => Microsoft.Maui.ApplicationModel.Platform.CurrentActivity!);
                }));
#endif
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
        builder.Services.AddSingleton<ITokenStorage, StorageBackedTokenStorage>();
        builder.Services.AddSingleton<TokenManager>();

        // تسجيل خدمة موافقة التتبع أولاً (يجب أن تكون قبل AddAshareClients)
        builder.Services.AddSingleton<ITrackingConsentService, TrackingConsentService>();
        builder.Services.AddTransient<TrackingConsentInterceptor>();

        // Device Info Provider للحصول على معلومات الجهاز في تقارير الأخطاء
        builder.Services.AddSingleton<IDeviceInfoProvider, MauiDeviceInfoProvider>();

        builder.Services.AddAshareClients(apiBaseUrl, options =>
        {
            options.TokenProvider = sp => sp.GetRequiredService<TokenManager>();
            // إضافة interceptor موافقة التتبع لإرسال الهيدر مع كل طلب
            options.AddHandler<TrackingConsentInterceptor>();
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

        // platform + version are sent with every AppConfig snapshot request so the
        // server can evaluate Feature Flags' Platform/MinVersion/MaxVersion rules.
        var platformName =
#if ANDROID
            "android";
#elif IOS
            "ios";
#elif MACCATALYST
            "macos";
#elif WINDOWS
            "windows";
#else
            "maui";
#endif
        builder.Services.AddAshareServices(
            appVersion: AppInfo.VersionString,
            platform: platformName);

        // Override VersionCheckService registration with explicit mobile app code
        // This is needed because OperatingSystem.IsAndroid() doesn't work correctly in Blazor Hybrid
        builder.Services.AddScoped<Ashare.Shared.Services.VersionCheckService>(sp =>
            new Ashare.Shared.Services.VersionCheckService(
                sp.GetRequiredService<ACommerce.Client.Versions.VersionsClient>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Ashare.Shared.Services.VersionCheckService>>(),
                Ashare.Shared.Services.VersionCheckService.MobileAppCode));

        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddSingleton<GuestModeService>();
        builder.Services.AddScoped<IAppNavigationService, AppNavigationService>();
        builder.Services.AddSingleton<SpaceDataService>();
        builder.Services.AddSingleton<ITimezoneService, DeviceTimezoneService>();
        builder.Services.AddSingleton<IPaymentService, MauiPaymentService>();
        builder.Services.AddSingleton<IAppVersionService, AppVersionService>();
        builder.Services.AddSingleton<AppLifecycleService>();
        builder.Services.AddSingleton<Ashare.Shared.Services.IAppLifecycleService>(sp =>
            sp.GetRequiredService<AppLifecycleService>());

        // Attribution Capture Service (for deep link tracking)
        builder.Services.AddSingleton<IAttributionCaptureService, AttributionCaptureService>();

        // Media Picker Service (for camera and gallery access)
        builder.Services.AddSingleton<IMediaPickerService, MauiMediaPickerService>();

        // Image Compression Service (for compressing images before upload)
        builder.Services.AddSingleton<IImageCompressionService, SkiaImageCompressionService>();

        // Push Notification Service (Firebase Cloud Messaging)
        builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();

        builder.Services.AddMockAnalytics();

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
