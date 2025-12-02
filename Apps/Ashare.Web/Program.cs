using Ashare.Shared.Services;
using Ashare.Web;
using Ashare.Web.Components;
using Ashare.Web.Services;
using ACommerce.Client.Categories;
using ACommerce.Client.Products;
using ACommerce.Client.ProductListings;
using ACommerce.Client.Orders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure API Base URL
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

// Register HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Register API Clients
builder.Services.AddScoped<CategoriesClient>();
builder.Services.AddScoped<CategoryAttributesClient>();
builder.Services.AddScoped<ProductsClient>();
builder.Services.AddScoped<ProductListingsClient>();
builder.Services.AddScoped<OrdersClient>();

// Register App Services
builder.Services.AddScoped<IStorageService, BrowserStorageService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<AshareApiService>();
builder.Services.AddScoped<SpaceDataService>();
builder.Services.AddScoped<IAppNavigationService, WebNavigationService>();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Ashare.Shared._Imports).Assembly);

app.Run();
