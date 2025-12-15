using Ashare.Admin.Components;
using Ashare.Admin.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Syncfusion.Blazor;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NMaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZfdXVVRmZdV0B/XUQ=");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSyncfusionBlazor();
builder.Services.AddMemoryCache();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AdminAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AdminAuthStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped(sp => new HttpClient());

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
