using ACommerce.Blazor.Shop;
using ACommerce.Blazor.Shop.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Cart;
using ACommerce.Client.Categories;
using ACommerce.Client.Chats;
using ACommerce.Client.ContactPoints;
using ACommerce.Client.Files;
using ACommerce.Client.Notifications;
using ACommerce.Client.Orders;
using ACommerce.Client.Payments;
using ACommerce.Client.ProductListings;
using ACommerce.Client.Products;
using ACommerce.Client.Profiles;
using ACommerce.Client.Realtime;
using ACommerce.Client.Shipping;
using ACommerce.Client.Vendors;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Syncfusion.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Base API URL
const string baseUrl = "http://localhost:5001/api/";

// HTTP Client
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseUrl) });

// ACommerce Client SDKs (simplified - direct registration)
// TODO: Implement proper service discovery with ServiceRegistry
builder.Services.AddScoped<AuthClient>();
builder.Services.AddScoped<ContactPointsClient>();
builder.Services.AddScoped<ProductsClient>();
builder.Services.AddScoped<ProductListingsClient>();
builder.Services.AddScoped<CartClient>();
builder.Services.AddScoped<OrdersClient>();
builder.Services.AddScoped<PaymentsClient>();
builder.Services.AddScoped<ShippingClient>();
builder.Services.AddScoped<VendorsClient>();
builder.Services.AddScoped<ProfilesClient>();
builder.Services.AddScoped<NotificationsClient>();
builder.Services.AddScoped<ChatsClient>();
builder.Services.AddScoped<RealtimeClient>();
builder.Services.AddScoped<FilesClient>();
builder.Services.AddScoped<CategoriesClient>();

// Syncfusion Blazor
builder.Services.AddSyncfusionBlazor();

// App Services
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<CartStateService>();
builder.Services.AddScoped<NotificationService>();

await builder.Build().RunAsync();
