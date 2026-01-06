using Microsoft.EntityFrameworkCore;
using Restaurant.Vendor.Api;
using Restaurant.Vendor.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// === Database ===
builder.Services.AddDbContext<VendorDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=restaurant_vendor.db"));

// === Controllers ===
builder.Services.AddControllers();

// === SignalR ===
builder.Services.AddSignalR();

// === Swagger ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Restaurant Vendor API", Version = "v1" });
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
    var db = scope.ServiceProvider.GetRequiredService<VendorDbContext>();
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
app.MapHub<VendorOrdersHub>("/hubs/vendor-orders");

app.Run();
