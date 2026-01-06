using Microsoft.EntityFrameworkCore;
using Restaurant.Customer.Api;
using Restaurant.Customer.Api.Hubs;
using Restaurant.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// === Database ===
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=restaurant_customer.db"));

// === Services ===
builder.Services.AddScoped<DeliveryCalculator>();
builder.Services.AddScoped<RestaurantAvailabilityService>();

// === Controllers ===
builder.Services.AddControllers();

// === SignalR ===
builder.Services.AddSignalR();

// === Swagger ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Restaurant Customer API", Version = "v1" });
});

// === CORS ===
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

// === Ensure Database Created ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
    db.Database.EnsureCreated();
}

// === Pipeline ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderTrackingHub>("/hubs/order-tracking");

app.Run();
