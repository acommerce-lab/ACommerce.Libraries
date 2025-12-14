using Ashare.Shared.Services;
using ACommerce.Templates.Customer.Services;
using ACommerce.Templates.Customer.Services.Analytics;
using Ashare.Web.Components;
using Ashare.Web.Services;
using ACommerce.Client.Auth;
using ACommerce.Client.Auth.Extensions;
using ACommerce.Client.Nafath;
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
using ACommerce.Client.Payments;
using ACommerce.Client.Subscriptions;
using ACommerce.ServiceRegistry.Client.Extensions;
using Ashare.Web;
using ACommerce.Client.Core.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Memory Cache for ListingsCacheService (Singleton)
builder.Services.AddMemoryCache();

// ═══════════════════════════════════════════════════════════════════
// API Configuration - يقرأ من HostSettings أولاً ثم ApiSettings للتوافق
// ═══════════════════════════════════════════════════════════════════
var apiBaseUrl = builder.Configuration["HostSettings:BaseUrl"] 
    ?? builder.Configuration["ApiSettings:BaseUrl"] 
    ?? "https://ashareapi-hygabpf3ajfmevfs.canadaeast-01.azurewebsites.net";

Console.WriteLine($"🌐 API Base URL: {apiBaseUrl}");

// ═══════════════════════════════════════════════════════════════════
// Client SDKs with Service Discovery (Predefined Services)
// ═══════════════════════════════════════════════════════════════════

// Storage Service (Browser localStorage implementation) - يجب تسجيله قبل TokenManager
// Scoped لأنه يستخدم IJSRuntime الذي هو per-circuit في Blazor Server
builder.Services.AddScoped<IStorageService, BrowserStorageService>();

// Token Storage (يستخدم IStorageService للتخزين الدائم)
builder.Services.AddScoped<ITokenStorage, TokenStorageService>();

// Token Manager - Scoped في الويب لتوافق lifetimes
// Note: كل circuit (اتصال) له TokenManager خاص، لكن التوكن محفوظ في localStorage
builder.Services.AddScoped<TokenManager>();

// Scoped Token Provider - singleton wrapper للوصول إلى TokenManager من HttpClient
builder.Services.AddSingleton<ScopedTokenProvider>();
builder.Services.AddSingleton<ITokenProvider>(sp => sp.GetRequiredService<ScopedTokenProvider>());

// ACommerce Client مع خدمات محددة مسبقاً
// يسجل الخدمات في Cache محلي لاستخدامها من قبل DynamicHttpClient
builder.Services.AddACommerceClientWithServices(
    services =>
    {
        // تسجيل خدمة Marketplace - تستخدمها معظم الـ Clients
        services.AddService("Marketplace", apiBaseUrl);

        // تسجيل خدمة Ashare - للـ SignalR و الإشعارات
        services.AddService("Ashare", apiBaseUrl);

        // تسجيل خدمة Payments - للدفع عبر Noon وغيرها
        services.AddService("Payments", apiBaseUrl);

        // تسجيل خدمة Files - لرفع الملفات والصور
        services.AddService("Files", apiBaseUrl);
    },
    options =>
    {
        options.TimeoutSeconds = 120; // 2 minutes for large operations like listing creation
        // تفعيل Authentication لإرسال التوكن مع كل طلب
        options.EnableAuthentication = true;
        // استخدام ScopedTokenProvider للوصول إلى TokenManager الـ scoped
        options.TokenProvider = sp => sp.GetRequiredService<ScopedTokenProvider>();
    });

// Authentication Client (يستخدم TokenManager المسجل أعلاه)
builder.Services.AddScoped<AuthClient>();

// Nafath Client (مصادقة نفاذ)
builder.Services.AddNafathClient();

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

// Subscriptions Client (Host/Vendor Subscription Plans)
builder.Services.AddScoped<SubscriptionClient>();

// Payments Client (Payment Gateway Integration)
builder.Services.AddScoped<PaymentsClient>();

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

// HttpClient for direct API calls (used by CreateListingPage for image upload)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// ═══════════════════════════════════════════════════════════════════
// App Services
// ═══════════════════════════════════════════════════════════════════

// Localization (AR, EN, UR)
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

// Theme Service (Dark/Light Mode)
builder.Services.AddScoped<ThemeService>();

// Guest Mode Service (allows browsing without login)
builder.Services.AddSingleton<GuestModeService>();

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
builder.Services.AddScoped<PendingListingService>();

// ═══════════════════════════════════════════════════════════════════
// Payment Service (خدمة الدفع)
// ═══════════════════════════════════════════════════════════════════
builder.Services.AddScoped<IPaymentService, WebPaymentService>();

// ═══════════════════════════════════════════════════════════════════
// Analytics Services (Meta, Google, TikTok, Snapchat)
// ═══════════════════════════════════════════════════════════════════
builder.Services.AddAshareAnalytics(builder.Configuration);

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
