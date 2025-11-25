using ACommerce.ServiceRegistry.Core.Extensions;
using ACommerce.ServiceRegistry.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// ✨ Add Service Registry Core
builder.Services.AddServiceRegistryCore();

// Background Service لفحص صحة الخدمات
builder.Services.AddHostedService<HealthCheckBackgroundService>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new()
	{
		Title = "ACommerce Service Registry",
		Version = "v1",
		Description = "Service Discovery & Registry for Microservices - لا توجد نقطة فشل حرجة! ✨"
	});
});

// CORS
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader();
	});
});

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Health Check Endpoint لنفس الـ Registry
app.MapGet("/health", () => Results.Ok(new
{
	status = "healthy",
	timestamp = DateTime.UtcNow,
	service = "ACommerce.ServiceRegistry"
}));

app.Run();
