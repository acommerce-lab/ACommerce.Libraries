using ACommerce.Payments.Abstractions.Contracts;
using ACommerce.Payments.Moyasar.Services;
using ACommerce.Payments.Moyasar.Models;
using ACommerce.Shipping.Abstractions.Contracts;
using ACommerce.Shipping.Mock.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers from libraries (ZERO custom controllers!)
// Note: MediatR, AutoMapper, FluentValidation are automatically inherited
// from the referenced library projects (via SharedKernel.CQRS)
builder.Services.AddControllers()
	.AddApplicationPart(typeof(ACommerce.Profiles.Api.Controllers.ProfilesController).Assembly)
	.AddApplicationPart(typeof(ACommerce.Vendors.Api.Controllers.VendorsController).Assembly)
	.AddApplicationPart(typeof(ACommerce.Catalog.Listings.Api.Controllers.ProductListingsController).Assembly)
	.AddApplicationPart(typeof(ACommerce.Orders.Api.Controllers.OrdersController).Assembly);

// In-Memory Database للتجربة
builder.Services.AddDbContext<DbContext>(options =>
	options.UseInMemoryDatabase("MarketplaceDb"));

// Repository Factory
builder.Services.AddScoped<ACommerce.SharedKernel.Abstractions.Repositories.IRepositoryFactory,
	ACommerce.SharedKernel.Infrastructure.EFCores.Factories.RepositoryFactory>();

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

// Configure pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ACommerce Marketplace API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Simple health check
app.MapGet("/", () => new
{
	Service = "ACommerce Marketplace API",
	Version = "1.0.0",
	Status = "Running",
	Endpoints = new[]
	{
		"/api/profiles",
		"/api/vendors",
		"/api/productlistings",
		"/api/cart",
		"/api/orders"
	}
});

app.Run();
