using Serilog;
using Microsoft.EntityFrameworkCore;
using ACommerce.SharedKernel.Infrastructure.EFCores.Context;
using ACommerce.SharedKernel.Infrastructure.EFCores.Extensions;
using ACommerce.SharedKernel.CQRS.Extensions;
using ACommerce.Authentication.JWT;
using ACommerce.Authentication.Users.Abstractions;
using ACommerce.Authentication.TwoFactor.SessionStore.InMemory;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Factories;
using ACommerce.Realtime.SignalR.Hubs;
using ACommerce.Realtime.SignalR.Extensions;
using ACommerce.Locations.Extensions;

// Service Registry & Messaging
using ACommerce.ServiceRegistry.Core.Extensions;
using ACommerce.ServiceRegistry.Server.Controllers;
using ACommerce.ServiceRegistry.Abstractions.Interfaces;
using ACommerce.ServiceRegistry.Abstractions.Models;
using ACommerce.Messaging.InMemory.Extensions;
using ACommerce.Messaging.SignalR.Hub.Extensions;
using ACommerce.Messaging.SignalR.Hub.Hubs;
using ACommerce.Notifications.Core.Extensions;
using ACommerce.Notifications.Channels.InApp.Extensions;
using ACommerce.Notifications.Messaging.Extensions;
using ACommerce.Authentication.Abstractions.Contracts;
using ACommerce.Authentication.AspNetCore.Services;
using ACommerce.Authentication.Messaging.Extensions;
using ACommerce.Notifications.Abstractions.Enums;

// Controllers from libraries
using ACommerce.Profiles.Api.Controllers;
using ACommerce.Vendors.Api.Controllers;
using ACommerce.Catalog.Products.Api.Controllers;
using ACommerce.Catalog.Listings.Api.Controllers;
using ACommerce.Orders.Api.Controllers;
using ACommerce.Locations.Api.Controllers;
using ACommerce.Notifications.Recipients.Api.Controllers;
using ACommerce.Payments.Api.Controllers;

// Files
using ACommerce.Files.Storage.Local.Extensions;

// Order specific
using Order.Api.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/order-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Order API - Offers & Deals Platform...");

    var builder = WebApplication.CreateBuilder(args);

    // Port configuration
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    Log.Information("Listening on port {Port}", port);

    // Logging
    builder.Host.UseSerilog();

    // Memory Cache
    builder.Services.AddMemoryCache();
    builder.Services.AddHttpContextAccessor();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // Controllers
    builder.Services.AddControllers()
        .AddApplicationPart(System.Reflection.Assembly.GetExecutingAssembly())
        .AddApplicationPart(typeof(ProfilesController).Assembly)
        .AddApplicationPart(typeof(VendorsController).Assembly)
        .AddApplicationPart(typeof(ProductsController).Assembly)
        .AddApplicationPart(typeof(ProductListingsController).Assembly)
        .AddApplicationPart(typeof(OrdersController).Assembly)
        .AddApplicationPart(typeof(LocationSearchController).Assembly)
        .AddApplicationPart(typeof(ContactPointsController).Assembly)
        .AddApplicationPart(typeof(PaymentsController).Assembly)
        .AddApplicationPart(typeof(RegistryController).Assembly);

    builder.Services.AddEndpointsApiExplorer();

    // Database (SQLite for development)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(connectionString ?? "Data Source=order.db");
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    });
    builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

    // Repository & Unit of Work
    builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
    builder.Services.AddScoped(typeof(IBaseAsyncRepository<>), typeof(BaseAsyncRepository<>));

    // Authentication (JWT)
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddSingleton<IUserProvider, InMemoryUserProvider>();

    // Two-Factor (SMS Mock)
    builder.Services.AddInMemoryTwoFactorSessionStore();
    builder.Services.AddScoped<IAuthenticationEventPublisher, MessagingAuthenticationEventPublisher>();

    // Service Registry & Messaging
    builder.Services.AddServiceRegistryCore();
    builder.Services.AddInMemoryMessaging("Order");
    builder.Services.AddMessagingHub();

    // Notifications
    builder.Services.AddNotificationCore(builder.Configuration);
    builder.Services.AddInAppNotifications();
    builder.Services.AddInMemoryNotificationPublisher();
    builder.Services.AddNotificationMessaging();
    builder.Services.AddAuthenticationMessaging(options =>
    {
        options.NotificationChannels = [NotificationChannel.InApp];
        options.NotifyOnInitiation = false;
    });

    // CQRS
    builder.Services.AddSharedKernelCQRS(AppDomain.CurrentDomain.GetAssemblies());

    // SignalR
    builder.Services.AddACommerceSignalR<NotificationHub, INotificationClient>();
    builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    });

    // Location Services
    builder.Services.AddACommerceLocations();

    // File Storage (Local)
    builder.Services.AddLocalFileStorage(builder.Configuration);

    // Order Seed Service
    builder.Services.AddScoped<OrderSeedDataService>();

    // Swagger
    builder.Services.AddSwaggerGen(options =>
    {
        options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
        options.SwaggerDoc("v1", new()
        {
            Title = "Order API - اوردر",
            Version = "v1.0.0",
            Description = @"
# اوردر - تطبيق العروض للكافيهات والمطاعم

## الميزات
- عرض العروض اليومية من الكافيهات والمطاعم
- البحث بالموقع الجغرافي
- طلب للاستلام من الكاشير
- طلب للتوصيل للسيارة مع مشاركة الموقع
- الدفع نقدي أو بالبطاقة
- مواعيد توافر المتاجر

## نقاط النهاية الرئيسية
- `/api/auth/*` - تسجيل الدخول برقم الهاتف
- `/api/profiles/*` - إدارة الملف الشخصي
- `/api/vendors/*` - المتاجر (كافيهات/مطاعم)
- `/api/productlistings/*` - العروض المتاحة
- `/api/orders/*` - الطلبات
- `/api/locations/*` - المواقع الجغرافية
"
        });

        options.AddSecurityDefinition("Bearer", new()
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "JWT Authorization"
        });

        options.AddSecurityRequirement(new()
        {
            {
                new()
                {
                    Reference = new()
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Swagger
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1.0");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "اوردر API";
    });

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // SignalR Hub
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapMessagingHub();

    // Database initialization
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Log.Information("Creating database...");
        await dbContext.Database.EnsureCreatedAsync();

        // Seed data
        Log.Information("Seeding initial data...");
        var seedService = scope.ServiceProvider.GetRequiredService<OrderSeedDataService>();
        await seedService.SeedAsync();
        Log.Information("Database ready!");
    }

    // Health check
    app.MapGet("/health", () => Results.Ok(new
    {
        Service = "Order API",
        Status = "Healthy",
        Version = "1.0.0",
        Timestamp = DateTime.UtcNow
    }));

    // Service Registry
    var serviceBaseUrl = builder.Configuration["HostSettings:BaseUrl"] ?? $"http://localhost:{port}";
    using (var scope = app.Services.CreateScope())
    {
        var registry = scope.ServiceProvider.GetRequiredService<IServiceRegistry>();
        await registry.RegisterAsync(new ServiceRegistration
        {
            ServiceName = "Order",
            Version = "v1",
            BaseUrl = serviceBaseUrl,
            Environment = app.Environment.EnvironmentName,
            EnableHealthCheck = true,
            HealthCheckPath = "/health"
        });

        // Also register as Marketplace for client compatibility
        await registry.RegisterAsync(new ServiceRegistration
        {
            ServiceName = "Marketplace",
            Version = "v1",
            BaseUrl = serviceBaseUrl,
            Environment = app.Environment.EnvironmentName
        });

        Log.Information("Service registered at {BaseUrl}", serviceBaseUrl);
    }

    Log.Information("Order API started successfully!");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
