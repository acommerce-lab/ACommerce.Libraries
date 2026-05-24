using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Nadena.Shared.Extensions;
using Nadena.Shared.Services;
using Nadena.Web;
using Nadena.Web.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Core.Interceptors;
using ACommerce.Client.Core.Storage;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Services.Analytics;
using ACommerce.Templates.Customer.Services.Localization;
using ACommerce.ServiceRegistry.Client.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Root components
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMemoryCache();

// API base URL (read from wwwroot/appsettings.json -> HostSettings:BaseUrl)
var apiBaseUrl = builder.Configuration["HostSettings:BaseUrl"]
    ?? builder.Configuration["ApiSettings:BaseUrl"]
    ?? "https://api.ashare.sa";

Console.WriteLine($"🌐 API Base URL: {apiBaseUrl}");

// Storage & Auth
builder.Services.AddScoped<IStorageService, BrowserStorageService>();
builder.Services.AddScoped<ITokenStorage, StorageBackedTokenStorage>();
builder.Services.AddScoped<TokenManager>();
builder.Services.AddSingleton<ScopedTokenProvider>();
builder.Services.AddSingleton<ITokenProvider>(sp => sp.GetRequiredService<ScopedTokenProvider>());

// API Clients
builder.Services.AddNadenaClients(apiBaseUrl, options =>
{
    options.TokenProvider = sp => sp.GetRequiredService<ScopedTokenProvider>();
});

// App Services
builder.Services.AddNadenaServices();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddSingleton<GuestModeService>();
builder.Services.AddScoped<IAppNavigationService, WebNavigationService>();
builder.Services.AddSingleton<WebAppLifecycleService>();
builder.Services.AddSingleton<IAppLifecycleService>(sp => sp.GetRequiredService<WebAppLifecycleService>());
builder.Services.AddScoped<SpaceDataService>();
builder.Services.AddScoped<ITimezoneService, BrowserTimezoneService>();
builder.Services.AddScoped<IPaymentService, WebPaymentService>();
builder.Services.AddSingleton<IAppVersionService, WebAppVersionService>();
builder.Services.AddScoped<IMediaPickerService, WebMediaPickerService>();
builder.Services.AddScoped<IImageCompressionService, WebImageCompressionService>();
builder.Services.AddSingleton<ITrackingConsentService, WebTrackingConsentService>();
builder.Services.AddSingleton<ACommerce.Client.Files.IDeviceInfoProvider, ACommerce.Client.Files.DefaultDeviceInfoProvider>();
builder.Services.AddACommerceAnalytics(builder.Configuration);
builder.Services.AddLocalizationValidation();

// HttpClient pointing at the backend API (requires CORS enabled on the API)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

var host = builder.Build();

// Initialize predefined service cache so discovery uses the cache (no registry server call from the browser)
host.Services.InitializeServiceCache();
// false = لا توقف الإقلاع عند وجود مفتاح ترجمة ناقص (يسجّل تحذيراً فقط)
host.Services.ValidateLocalization(false);

await host.RunAsync();
