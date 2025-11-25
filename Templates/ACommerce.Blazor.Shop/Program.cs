using ACommerce.Blazor.Shop;
using ACommerce.Client.Auth.Extensions;
using ACommerce.Client.Core.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Syncfusion.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ✨ Service Registry URL
const string registryUrl = "http://localhost:5100";

// ✨ ACommerce Client SDKs - سطر واحد لكل خدمة!
builder.Services.AddAuthClient(registryUrl);

builder.Services.AddACommerceClient(registryUrl, options =>
{
	options.EnableAuthentication = true;
	options.EnableLocalization = true;
	options.EnableRetry = true;
});

// ✨ Syncfusion Blazor
builder.Services.AddSyncfusionBlazor();

// ✨ App Services
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<CartStateService>();
builder.Services.AddScoped<NotificationService>();

await builder.Build().RunAsync();
