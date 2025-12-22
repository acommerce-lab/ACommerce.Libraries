using Ashare.Admin.Components;
using Ashare.Admin.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Syncfusion.Blazor;
using ACommerce.Client.Core.Extensions;
using ACommerce.Client.Versions;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH9ecnRVR2RcUEJ2W0tWYEg=");

var builder = WebApplication.CreateBuilder(args);

// API Base URL
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://ashare-api-130415035604.me-central2.run.app";

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSyncfusionBlazor();
builder.Services.AddMemoryCache();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AdminAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AdminAuthStateProvider>());
builder.Services.AddHttpClient<AuthService>((sp, client) =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped(sp => new HttpClient());

// ACommerce Client SDK
builder.Services.AddACommerceStaticClient(apiBaseUrl);
builder.Services.AddScoped<VersionsClient>();

builder.Services.AddScoped<AdminApiService>();
builder.Services.AddScoped<MarketingAnalyticsService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
