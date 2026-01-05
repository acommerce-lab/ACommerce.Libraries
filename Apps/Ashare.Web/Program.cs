using Ashare.Shared.Extensions;
using Ashare.Shared.Services;
using Ashare.Web.Components;
using Ashare.Web.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Core.Interceptors;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Services.Analytics;
using ACommerce.Templates.Customer.Services.Localization;
using ACommerce.ServiceRegistry.Client.Extensions;
using ACommerce.Client.Core.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMemoryCache();

var apiBaseUrl = builder.Configuration["HostSettings:BaseUrl"] 
    ?? builder.Configuration["ApiSettings:BaseUrl"] 
    ?? "https://ashare-api-130415035604.me-central2.run.app";

Console.WriteLine($"🌐 API Base URL: {apiBaseUrl}");

builder.Services.AddScoped<IStorageService, BrowserStorageService>();
builder.Services.AddScoped<ITokenStorage, StorageBackedTokenStorage>();
builder.Services.AddScoped<TokenManager>();
builder.Services.AddSingleton<ScopedTokenProvider>();
builder.Services.AddSingleton<ITokenProvider>(sp => sp.GetRequiredService<ScopedTokenProvider>());

builder.Services.AddAshareClients(apiBaseUrl, options =>
{
    options.TokenProvider = sp => sp.GetRequiredService<ScopedTokenProvider>();
});

builder.Services.AddAshareServices();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddSingleton<GuestModeService>();
builder.Services.AddScoped<IAppNavigationService, WebNavigationService>();
builder.Services.AddScoped<SpaceDataService>();
builder.Services.AddScoped<ITimezoneService, BrowserTimezoneService>();
builder.Services.AddScoped<IPaymentService, WebPaymentService>();
builder.Services.AddSingleton<IAppVersionService, WebAppVersionService>();
builder.Services.AddACommerceAnalytics(builder.Configuration);
builder.Services.AddLocalizationValidation();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

var app = builder.Build();

app.Services.InitializeServiceCache();
app.Services.ValidateLocalization(app.Environment.IsDevelopment());

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Ashare.Shared._Imports).Assembly);

app.Run();
