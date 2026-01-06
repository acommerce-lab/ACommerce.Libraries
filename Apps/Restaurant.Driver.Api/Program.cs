using Microsoft.EntityFrameworkCore;
using Restaurant.Driver.Api;
using Restaurant.Driver.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// === Database ===
builder.Services.AddDbContext<DriverDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=restaurant_driver.db"));

// === Controllers ===
builder.Services.AddControllers();

// === SignalR ===
builder.Services.AddSignalR();

// === Swagger ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Restaurant Driver API", Version = "v1" });
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
    var db = scope.ServiceProvider.GetRequiredService<DriverDbContext>();
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
app.MapHub<DriverOrdersHub>("/hubs/driver-orders");

app.Run();
