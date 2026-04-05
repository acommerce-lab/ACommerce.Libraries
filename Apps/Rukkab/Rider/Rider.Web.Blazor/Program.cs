using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Rukkab.Rider.Web.Blazor;
using ACommerce.ServiceRegistry.Client.Extensions;
using ACommerce.Client.Realtime;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Rukkab.Rider.Web.Blazor.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register service registry entries pointing to the local Rider API
builder.Services.AddServiceRegistryWithPredefinedServices(opts =>
{
	opts.AddService("Rider.Api", "http://127.0.0.1:5001");
});

// Make RealtimeClient available via DI so pages can prefer it
builder.Services.AddSingleton<RealtimeClient>();

// Build the host, initialize the predefined service cache (so discovery uses the cache
// instead of trying to call a registry server from the browser), then run.
var host = builder.Build();
host.Services.InitializeServiceCache();
await host.RunAsync();
