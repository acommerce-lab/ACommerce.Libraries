using Ashare.App.Services;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Themes;
using Microsoft.Extensions.Logging;

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

        // Add ACommerce Customer Template with Ashare theme
        builder.Services.AddACommerceCustomerTemplate(options =>
        {
            // Ashare brand colors - Sky Blue theme
            options.Colors.Primary = "#0EA5E9";      // Sky-500
            options.Colors.PrimaryDark = "#0284C7";  // Sky-600
            options.Colors.PrimaryLight = "#38BDF8"; // Sky-400
            options.Colors.Secondary = "#8B5CF6";    // Violet-500
            options.Colors.Success = "#10B981";      // Emerald-500
            options.Colors.Error = "#EF4444";        // Red-500
            options.Colors.Warning = "#F59E0B";      // Amber-500
            options.Colors.Info = "#06B6D4";         // Cyan-500

            // Set text direction for Arabic
            options.Direction = TextDirection.RTL;

            // Start with light mode
            options.Mode = ThemeMode.Light;
        });

        // Add app services
        builder.Services.AddScoped<IAppNavigationService, AppNavigationService>();
        builder.Services.AddSingleton<SpaceDataService>();

        return builder.Build();
    }
}
