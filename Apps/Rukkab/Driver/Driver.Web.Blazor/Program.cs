using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Rukkab.Driver.Web.Blazor;
using ACommerce.ServiceRegistry.Client.Extensions;
using ACommerce.Client.Realtime;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Rukkab.Driver.Web.Blazor.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Http client for app assets
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register a lightweight service registry client with predefined service entries
// so the RealtimeClient can discover backend service endpoints in local dev.
builder.Services.AddServiceRegistryWithPredefinedServices(opts =>
{
	// register the Driver API so RealtimeClient can discover it
	opts.AddService("Driver.Api", "http://127.0.0.1:5002");
});

// Register the RealtimeClient for centralized SignalR handling in the Blazor app
builder.Services.AddSingleton<RealtimeClient>();

// Build the host and initialize the predefined service cache so discovery returns
// cached endpoints instead of attempting HTTP calls to a registry server from the browser.
var host = builder.Build();
host.Services.InitializeServiceCache();
await host.RunAsync();
