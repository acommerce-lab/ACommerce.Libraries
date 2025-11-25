using ACommerce.Payments.Abstractions.Contracts;
using ACommerce.Payments.Moyasar.Services;
using ACommerce.Payments.Moyasar.Models;
using ACommerce.Shipping.Abstractions.Contracts;
using ACommerce.Shipping.Mock.Services;
using ACommerce.MarketplaceApi.Services;
using ACommerce.SharedKernel.Infrastructure.EFCores.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers from libraries + Auth controller (custom)
// Note: MediatR, AutoMapper, FluentValidation are automatically inherited
// from the referenced library projects (via SharedKernel.CQRS)
builder.Services.AddControllers()
	.AddApplicationPart(typeof(ACommerce.Profiles.Api.Controllers.ProfilesController).Assembly)
	.AddApplicationPart(typeof(ACommerce.Vendors.Api.Controllers.VendorsController).Assembly)
	.AddApplicationPart(typeof(ACommerce.Catalog.Products.Api.Controllers.ProductsController).Assembly) // ✅ Products!
	.AddApplicationPart(typeof(ACommerce.Catalog.Listings.Api.Controllers.ProductListingsController).Assembly)
	.AddApplicationPart(typeof(ACommerce.Orders.Api.Controllers.OrdersController).Assembly);

// ✨ Database - سطر واحد يكفي! Auto-Discovery لجميع Entities من جميع المكتبات
builder.Services.AddACommerceInMemoryDatabase("MarketplaceDb");

// Payment Provider
builder.Services.Configure<MoyasarOptions>(options =>
{
	options.ApiKey = builder.Configuration["Moyasar:ApiKey"] ?? "test_key";
	options.PublishableKey = builder.Configuration["Moyasar:PublishableKey"] ?? "test_pub_key";
	options.UseSandbox = true;
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPaymentProvider, MoyasarPaymentProvider>();

// Shipping Provider
builder.Services.AddScoped<IShippingProvider, MockShippingProvider>();

// Mock Authentication (للتجربة - في الإنتاج استخدم ACommerce.Authentication.JWT)
builder.Services.AddSingleton<MockAuthService>();

// Seed Data Service
builder.Services.AddScoped<SeedDataService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new() {
		Title = "ACommerce Marketplace API",
		Version = "v1",
		Description = "Multi-Vendor E-Commerce Backend - Built with ACommerce Libraries"
	});
});

var app = builder.Build();

// Seed test data
using (var scope = app.Services.CreateScope())
{
	var seedService = scope.ServiceProvider.GetRequiredService<SeedDataService>();
	await seedService.SeedAsync();
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ACommerce Marketplace API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Simple health check + Quick start guide
app.MapGet("/", () => new
{
	Service = "ACommerce Marketplace API",
	Version = "1.0.1",
	Status = "Running ✅",
	Message = "متجر متعدد البائعين كامل في ملف واحد!",
	QuickStart = new
	{
		Step1 = "افتح /swagger للوصول إلى واجهة Swagger",
		Step2 = "جرّب /api/auth/test-users لرؤية المستخدمين التجريبيين",
		Step3 = "سجل دخول عبر /api/auth/login",
		Step4 = "جرّب APIs: Products, Cart, Orders"
	},
	TestUsers = new[]
	{
		new { Email = "customer@example.com", Role = "Customer", Password = "123456" },
		new { Email = "vendor@example.com", Role = "Vendor", Password = "123456" },
		new { Email = "admin@example.com", Role = "Admin", Password = "123456" }
	},
	Endpoints = new[]
	{
		"/api/auth/login",
		"/api/auth/register",
		"/api/auth/test-users",
		"/api/profiles",
		"/api/vendors",
		"/api/products",
		"/api/productlistings",
		"/api/cart",
		"/api/orders"
	}
});

app.Run();
