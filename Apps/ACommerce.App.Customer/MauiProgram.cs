using ACommerce.App.Customer.Services;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Themes;
using Microsoft.Extensions.Logging;

namespace ACommerce.App.Customer;

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

		// Add ACommerce Customer Template with custom theme
		builder.Services.AddACommerceCustomerTemplate(options =>
		{
			// Customize theme colors
			options.Colors.Primary = "#2563eb";
			options.Colors.Secondary = "#64748b";
			options.Colors.Success = "#22c55e";
			options.Colors.Danger = "#ef4444";
			options.Colors.Warning = "#f59e0b";
			options.Colors.Info = "#3b82f6";

			// Set text direction for Arabic
			options.Direction = TextDirection.RTL;

			// Start with light mode
			options.Mode = ThemeMode.Light;
		});

		// Add app services
		builder.Services.AddSingleton<IAppNavigationService, AppNavigationService>();
		builder.Services.AddSingleton<MockDataService>();

		return builder.Build();
	}
}
