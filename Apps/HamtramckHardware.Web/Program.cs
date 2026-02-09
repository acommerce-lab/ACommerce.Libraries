using HamtramckHardware.Shared.Extensions;
using HamtramckHardware.Shared.Services;
using HamtramckHardware.Web.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Core.Interceptors;
using ACommerce.Client.Core.Storage;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Themes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure theme
builder.Services.AddACommerceCustomerTemplate(options =>
{
    options.Colors.Primary = HamtramckColors.Primary;
    options.Colors.PrimaryDark = HamtramckColors.PrimaryDark;
    options.Colors.PrimaryLight = HamtramckColors.PrimaryLight;
    options.Colors.Secondary = HamtramckColors.Secondary;
    options.Colors.Success = HamtramckColors.Success;
    options.Colors.Error = HamtramckColors.Error;
    options.Colors.Warning = HamtramckColors.Warning;
    options.Colors.Info = HamtramckColors.Info;
    options.Colors.Background = HamtramckColors.Background;
    options.Colors.Surface = HamtramckColors.Surface;
    options.Direction = TextDirection.LTR;
    options.Mode = ThemeMode.Light;
});

// API Configuration - Use local API in development
var useLocalApi = builder.Environment.IsDevelopment();
var apiConfig = ApiConfiguration.ForWeb(useLocalApi: useLocalApi);
Console.WriteLine($"[HamtramckHardware.Web] API Base URL: {apiConfig.BaseUrl}");

// Storage & Auth
builder.Services.AddScoped<IStorageService, BrowserStorageService>();
builder.Services.AddScoped<ITokenStorage, StorageBackedTokenStorage>();
builder.Services.AddScoped<TokenManager>();

// ScopedTokenProvider for HttpClient handlers (singleton wrapper for scoped TokenManager)
builder.Services.AddSingleton<ScopedTokenProvider>();
builder.Services.AddSingleton<ITokenProvider>(sp => sp.GetRequiredService<ScopedTokenProvider>());

// API Clients
builder.Services.AddHamtramckClients(apiConfig.BaseUrl, options =>
{
    options.TokenProvider = sp => sp.GetRequiredService<ScopedTokenProvider>();
});

// App Services
builder.Services.AddHamtramckServices();
builder.Services.AddScoped<IAppNavigationService, WebNavigationService>();
builder.Services.AddScoped<IAppVersionService, WebAppVersionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<HamtramckHardware.Web.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(HamtramckHardware.Shared._Imports).Assembly);

app.Run();
