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
using ACommerce.SharedKernel.CQRS.Extensions;
using Rukkab.Shared.Domain.Entities;
using Rukkab.Driver.Api.Services;
using Rukkab.Driver.Api.Hubs;
using ACommerce.Locations.Services;
using ACommerce.DriverMatching;
using ACommerce.RideLifecycle;

// Early startup marker to ensure the running binary is the one we edited.
System.Console.WriteLine($"[STARTUP] Driver.Program starting at {System.DateTime.UtcNow:o} (pid={System.Diagnostics.Process.GetCurrentProcess().Id})");
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

// Add MVC controllers support and Swagger/OpenAPI for developer experience
builder.Services.AddControllers()
    .AddApplicationPart(System.Reflection.Assembly.GetExecutingAssembly())
    .AddApplicationPart(typeof(ProfilesController).Assembly)
    .AddApplicationPart(typeof(OrdersController).Assembly)
    .AddApplicationPart(typeof(LocationSearchController).Assembly)
    .AddApplicationPart(typeof(ChatsController).Assembly)
    .AddApplicationPart(typeof(ContactPointsController).Assembly)
    .AddApplicationPart(typeof(PaymentsController).Assembly)
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

// CQRS/MediatR (register globally so handlers are available in all environments)
builder.Services.AddSharedKernelCQRS();

// Locations
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddACommerceLocationsInMemory();
    // Driver matching (in-memory for development)
    builder.Services.AddInMemoryDriverMatching();
    // Use persistent SQLite-backed orchestrator for MVP development
    builder.Services.AddDbContext<ACommerce.RideLifecycle.Persistence.RideDbContext>(opts => opts.UseSqlite("Data Source=rukkab-rides.db"));
    builder.Services.AddPersistentRideOrchestrator();
    // Chats + realtime hub for in-ride chat (development)
    builder.Services.AddChatsCore();
    builder.Services.AddACommerceSignalR<ChatHub, IChatClient>();
    // Realtime driver location ingestion (development)
    builder.Services.AddSingleton<IDriverLocationStore, InMemoryDriverLocationStore>();
}
else
{
    builder.Services.AddACommerceLocations();
}

// File storage (local for development)
builder.Services.AddLocalFileStorage(builder.Configuration);

// Register ACommerce DB context using the helper extension
var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION") ?? builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("Data Source="))
{
    builder.Services.AddACommerceSQLite(connectionString ?? "Data Source=rukkab-driver.db");
}
else
{
    builder.Services.AddACommerceDbContext(options => options.UseSqlite(connectionString));
}

// Register seed service
builder.Services.AddScoped<RukkabSeedDataService>();

var app = builder.Build();

// Register entity explicitly before DbContext model creation
ACommerce.SharedKernel.Abstractions.Entities.EntityDiscoveryRegistry.RegisterEntity<RideCategory>();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    var seeder = services.GetRequiredService<RukkabSeedDataService>();
    await seeder.SeedAsync();
}

app.MapGet("/", () => Results.Ok("Rukkab Driver API - healthy"));

// Debug: raw import endpoint (no model binding) for diagnosing route matching issues from local runners.
// This reads the raw request body, logs its length, and returns a simple OK so we can confirm the route is reachable.
app.MapPost("/api/internal/debug/import-ride-raw", async (Microsoft.AspNetCore.Http.HttpContext http, Microsoft.Extensions.Logging.ILogger<Program> logger) =>
{
    try
    {
        using var reader = new System.IO.StreamReader(http.Request.Body);
        var body = await reader.ReadToEndAsync();
        logger.LogInformation("DEBUG_RAW_IMPORT: received request body length {Length}", body?.Length ?? 0);
        System.Console.WriteLine($"[DEBUG] raw import body length: {body?.Length ?? 0}");
        return Results.Ok(new { received = true, length = body?.Length ?? 0 });
    }
    catch (System.Exception ex)
    {
        logger.LogError(ex, "failed reading raw import body");
        return Results.StatusCode(500);
    }
});

// Debug: expose registered route patterns so we can inspect what's actually mapped at runtime.
app.MapGet("/__debug/endpoints", (Microsoft.AspNetCore.Routing.EndpointDataSource ds) =>
{
    var routes = ds.Endpoints.OfType<Microsoft.AspNetCore.Routing.RouteEndpoint>()
        .Select(e => e.RoutePattern.RawText)
        .ToList();
    return Results.Ok(routes);
});

// Development-only: explicit endpoint to import a ride into the in-memory orchestrator.
// This is registered directly so local runners can reliably import rides when controller discovery
// doesn't expose the debug controller for any reason.
app.MapPost("/api/internal/debug/import-ride", async (IRideOrchestrator orchestrator, ACommerce.RideLifecycle.Models.Ride ride) =>
{
    try { System.Console.WriteLine($"[HANDLER] import-ride invoked. Ride null? {ride == null}"); } catch { }
    if (ride == null) return Results.BadRequest("ride body is required");
    if (orchestrator is ACommerce.RideLifecycle.InMemoryRideOrchestrator mem)
    {
        await mem.ImportRideAsync(ride);
        return Results.Ok();
    }

    var method = orchestrator.GetType().GetMethod("ImportRideAsync");
    if (method != null)
    {
        var task = (System.Threading.Tasks.Task)method.Invoke(orchestrator, new object[] { ride })!;
        await task;
        return Results.Ok();
    }

    return Results.BadRequest("orchestrator does not support import");
});

// Map controllers (if any) and enable Swagger UI in development or when configured
app.MapControllers();

// Map realtime hubs used by the frontend (ensure negotiate endpoints exist)
app.MapHub<ACommerce.Chats.Core.Hubs.ChatHub>("/hubs/chat");
app.MapHub<ACommerce.Realtime.SignalR.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<Rukkab.Driver.Api.Hubs.DriverLocationHub>("/hubs/driver-location");

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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Rukkab Driver API v1");
        options.RoutePrefix = "swagger";
    });
}

// Map realtime hubs
app.MapHub<DriverLocationHub>("/hubs/driver-location");

// Debug: print registered endpoints at startup so we can confirm which routes were actually mapped.
try
{
    var endpointDataSource = app.Services.GetService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
    if (endpointDataSource != null)
    {
        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (endpoint is Microsoft.AspNetCore.Routing.RouteEndpoint re)
            {
                System.Console.WriteLine($"[ENDPOINT] {re.RoutePattern.RawText}");
            }
            else
            {
                System.Console.WriteLine($"[ENDPOINT] {endpoint.DisplayName}");
            }
        }
    }
}
catch (System.Exception ex)
{
    System.Console.WriteLine($"[DEBUG] failed to enumerate endpoints: {ex.Message}");
}

app.Run();
