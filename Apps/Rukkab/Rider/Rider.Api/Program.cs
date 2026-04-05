using Microsoft.EntityFrameworkCore;
using ACommerce.SharedKernel.Infrastructure.EFCores.Context;
using ACommerce.SharedKernel.Infrastructure.EFCores.Extensions;
using ACommerce.Authentication.AspNetCore.Swagger;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Factories;
using ACommerce.Authentication.JWT;
using ACommerce.Profiles.Api.Controllers;
using ACommerce.Orders.Api.Controllers;
using ACommerce.ServiceRegistry.Server.Controllers;
using ACommerce.SharedKernel.Infrastructure.EFCore.Repositories;
using ACommerce.Authentication.Users.Abstractions;
using ACommerce.Chats.Api.Controllers;
using ACommerce.Notifications.Recipients.Api.Controllers;
using ACommerce.Locations.Api.Controllers;
using ACommerce.Payments.Api.Controllers;
using ACommerce.Realtime.SignalR.Hubs;
using ACommerce.Realtime.SignalR.Extensions;
using ACommerce.Notifications.Core.Extensions;
using ACommerce.Notifications.Channels.InApp.Extensions;
using ACommerce.Notifications.Messaging.Extensions;
using ACommerce.Messaging.InMemory.Extensions;
using ACommerce.Messaging.SignalR.Hub.Extensions;
using ACommerce.Files.Storage.Local.Extensions;
using ACommerce.Locations.Extensions;
using ACommerce.Chats.Core.Extensions;
using ACommerce.Chats.Core.Hubs;
using Rukkab.Shared.Domain.Entities;
using Rukkab.Rider.Api.Services;
using ACommerce.Locations.Services;
using ACommerce.DriverMatching;
using ACommerce.RideLifecycle;
using ACommerce.SharedKernel.CQRS.Extensions;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// CORS: allow local Blazor dev servers to call this API (preflight and SignalR negotiate)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalBlazor", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5003", "http://127.0.0.1:5004")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers()
    .AddApplicationPart(System.Reflection.Assembly.GetExecutingAssembly())
    .AddApplicationPart(typeof(ProfilesController).Assembly)
    .AddApplicationPart(typeof(OrdersController).Assembly)
    .AddApplicationPart(typeof(LocationSearchController).Assembly)
    .AddApplicationPart(typeof(ChatsController).Assembly)
    .AddApplicationPart(typeof(ContactPointsController).Assembly)
    .AddApplicationPart(typeof(RegistryController).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddACommerceSwagger();

// Repository + UoW
builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
builder.Services.AddScoped(typeof(IBaseAsyncRepository<>), typeof(BaseAsyncRepository<>));

// Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSingleton<IUserProvider, InMemoryUserProvider>();

// Messaging / Service Bus (in-memory for local dev)
builder.Services.AddInMemoryMessaging("Rukkab");
builder.Services.AddMessagingHub();

// Notifications
builder.Services.AddNotificationCore(builder.Configuration);
builder.Services.AddInAppNotifications();
builder.Services.AddInMemoryNotificationPublisher();
builder.Services.AddNotificationMessaging();

// SignalR / Realtime
builder.Services.AddACommerceSignalR<NotificationHub, INotificationClient>();
builder.Services.AddSignalR(options => { options.EnableDetailedErrors = builder.Environment.IsDevelopment(); });

// Locations
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddACommerceLocationsInMemory();
    // Driver matching (in-memory for development)
    builder.Services.AddInMemoryDriverMatching();
    // Use persistent SQLite-backed orchestrator for MVP development
    // Register the RideDbContext backed by a local SQLite file and then register the orchestrator.
    builder.Services.AddDbContext<ACommerce.RideLifecycle.Persistence.RideDbContext>(opts => opts.UseSqlite("Data Source=rukkab-rides.db"));
    // CQRS/MediatR (used by chat providers and some handlers)
    builder.Services.AddSharedKernelCQRS();
    // Chats + realtime hub for in-ride chat (development)
    builder.Services.AddChatsCore();
    builder.Services.AddACommerceSignalR<ChatHub, IChatClient>();
}
else
{
    builder.Services.AddACommerceLocations();
}

// Ensure a ride orchestrator is available for runs (persistent SQLite-backed orchestrator).
builder.Services.AddPersistentRideOrchestrator();

// File storage (local for development)
builder.Services.AddLocalFileStorage(builder.Configuration);

var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION") ?? builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("Data Source="))
{
    builder.Services.AddACommerceSQLite(connectionString ?? "Data Source=rukkab-rider.db");
}
else
{
    builder.Services.AddACommerceDbContext(options => options.UseSqlite(connectionString));
}

builder.Services.AddScoped<RukkabSeedDataService>();

var app = builder.Build();

ACommerce.SharedKernel.Abstractions.Entities.EntityDiscoveryRegistry.RegisterEntity<RideCategory>();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    var seeder = services.GetRequiredService<RukkabSeedDataService>();
    await seeder.SeedAsync();
}

app.MapGet("/", () => Results.Ok("Rukkab Rider API - healthy"));

// Map controllers (if any) and enable Swagger UI in development or when configured
app.MapControllers();

// Map realtime hubs used by the frontend (ensure negotiate endpoints exist)
app.MapHub<ACommerce.Chats.Core.Hubs.ChatHub>("/hubs/chat");
app.MapHub<ACommerce.Realtime.SignalR.Hubs.NotificationHub>("/hubs/notifications");

// Enable CORS for browser clients (must be before UseRouting/MapHub in some hosting scenarios)
app.UseCors("AllowLocalBlazor");

if (app.Environment.IsDevelopment() || configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    // Redirect legacy requests to the exact index.html file.
    app.Use(async (context, next) =>
    {
        if (string.Equals(context.Request.Path, "/swagger/index", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Redirect("/swagger/index.html");
            return;
        }

        await next();
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Rukkab Rider API v1");
        options.RoutePrefix = "swagger";
    });
}

app.Run();
