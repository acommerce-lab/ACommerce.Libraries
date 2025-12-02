using Ashare.Shared.Services;
using Ashare.Web.Components;
using Ashare.Web.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Auth.Extensions;
using ACommerce.Client.Cart;
using ACommerce.Client.Cart.Extensions;
using ACommerce.Client.Categories;
using ACommerce.Client.Categories.Extensions;
using ACommerce.Client.Chats;
using ACommerce.Client.ContactPoints;
using ACommerce.Client.ContactPoints.Extensions;
using ACommerce.Client.Core.Extensions;
using ACommerce.Client.Files;
using ACommerce.Client.Locations;
using ACommerce.Client.Locations.Extensions;
using ACommerce.Client.Notifications;
using ACommerce.Client.Orders;
using ACommerce.Client.Orders.Extensions;
using ACommerce.Client.ProductListings;
using ACommerce.Client.Products;
using ACommerce.Client.Products.Extensions;
using ACommerce.Client.Realtime;
using ACommerce.Client.Vendors;
using ACommerce.Client.Profiles;
using ACommerce.ServiceRegistry.Client.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ═══════════════════════════════════════════════════════════════════
// API Configuration
// ═══════════════════════════════════════════════════════════════════
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001";

// ═══════════════════════════════════════════════════════════════════
// Client SDKs with Service Discovery (Predefined Services)
// ═══════════════════════════════════════════════════════════════════

// Token Manager (singleton) - يجب تسجيله قبل AddACommerceClientWithServices
builder.Services.AddSingleton<TokenManager>();

// ACommerce Client مع خدمات محددة مسبقاً
// يسجل الخدمات في Cache محلي لاستخدامها من قبل DynamicHttpClient
builder.Services.AddACommerceClientWithServices(
    services =>
    {
        // تسجيل خدمة Marketplace - تستخدمها معظم الـ Clients
        services.AddService("Marketplace", apiBaseUrl);
    },
    options =>
    {
        options.TimeoutSeconds = 30;
        // تفعيل Authentication لإرسال التوكن مع كل طلب
        options.EnableAuthentication = true;
        options.TokenProvider = sp => sp.GetRequiredService<TokenManager>();
    });

// Authentication Client (يستخدم TokenManager المسجل أعلاه)
builder.Services.AddScoped<AuthClient>();

// Locations Client
builder.Services.AddLocationsClient(apiBaseUrl);

// ═══════════════════════════════════════════════════════════════════
// Catalog Clients (Spaces = Products with Dynamic Attributes)
// ═══════════════════════════════════════════════════════════════════

// Products Client (Spaces)
builder.Services.AddProductsClient(apiBaseUrl);

// Categories Client (Space Types)
builder.Services.AddCategoriesClient(apiBaseUrl);

// ProductListings Client (Owner Listings)
builder.Services.AddScoped<ProductListingsClient>();

// ═══════════════════════════════════════════════════════════════════
// Sales Clients (Bookings = Orders)
// ═══════════════════════════════════════════════════════════════════

// Orders Client (Bookings)
builder.Services.AddOrdersClient(apiBaseUrl);

// Cart Client
builder.Services.AddCartClient(apiBaseUrl);

// ═══════════════════════════════════════════════════════════════════
// Marketplace Clients (Hosts = Vendors)
// ═══════════════════════════════════════════════════════════════════

// Vendors Client (Hosts)
builder.Services.AddScoped<VendorsClient>();

// Profiles Client (User Profiles)
builder.Services.AddScoped<ProfilesClient>();

// ═══════════════════════════════════════════════════════════════════
// Communication Clients
// ═══════════════════════════════════════════════════════════════════

// Chats Client
builder.Services.AddScoped<ChatsClient>();

// Notifications Client
builder.Services.AddScoped<NotificationsClient>();

// ContactPoints Client
builder.Services.AddContactPointsClient(apiBaseUrl);

// Real-time Client (SignalR)
builder.Services.AddSingleton<RealtimeClient>();

// Files Client
builder.Services.AddScoped<FilesClient>();

// ═══════════════════════════════════════════════════════════════════
// App Services
// ═══════════════════════════════════════════════════════════════════

// Storage Service (Browser localStorage implementation)
builder.Services.AddScoped<IStorageService, BrowserStorageService>();

// Localization (AR, EN, UR)
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

// Theme Service (Dark/Light Mode)
builder.Services.AddScoped<ThemeService>();

// Navigation Service (Web implementation)
builder.Services.AddScoped<IAppNavigationService, WebNavigationService>();

// Space Data Service (mock data for development)
builder.Services.AddScoped<SpaceDataService>();

// Timezone Service (Browser implementation using JS interop)
builder.Services.AddScoped<ITimezoneService, BrowserTimezoneService>();

// ═══════════════════════════════════════════════════════════════════
// Ashare API Service (ربط التطبيق بالباك اند)
// ═══════════════════════════════════════════════════════════════════
builder.Services.AddScoped<AshareApiService>();

// Additional client registrations
builder.Services.AddScoped<CategoriesClient>();
builder.Services.AddScoped<CategoryAttributesClient>();
builder.Services.AddScoped<ProductsClient>();
builder.Services.AddScoped<ProductListingsClient>();
builder.Services.AddScoped<OrdersClient>();

var app = builder.Build();

// تهيئة Service Cache بالخدمات المحددة مسبقاً
app.Services.InitializeServiceCache();

// Configure the HTTP request pipeline.
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
