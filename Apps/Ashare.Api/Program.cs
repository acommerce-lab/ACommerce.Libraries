using Serilog;
using Microsoft.EntityFrameworkCore;
using ACommerce.SharedKernel.Infrastructure.EFCores.Context;
using ACommerce.SharedKernel.Infrastructure.EFCores.Extensions;
using ACommerce.SharedKernel.CQRS.Extensions;
using ACommerce.Authentication.JWT;
using ACommerce.Authentication.Users.Abstractions;
using ACommerce.Authentication.TwoFactor.Nafath;
using ACommerce.Authentication.TwoFactor.SessionStore.InMemory;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Factories;
using ACommerce.Realtime.SignalR.Hubs;
using ACommerce.Realtime.SignalR.Extensions;
using ACommerce.Chats.Core.Hubs;
using ACommerce.Chats.Core.Extensions;
using ACommerce.Locations.Extensions;
using ACommerce.Catalog.Products.Services;
using Ashare.Api.Services;

// Service Registry & Messaging (for development - self-hosting)
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
using ACommerce.Catalog.Attributes.Api.Controllers;
using ACommerce.Catalog.Units.Api.Controllers;
using ACommerce.Catalog.Currencies.Api.Controllers;
using ACommerce.Orders.Api.Controllers;
using ACommerce.Transactions.Core.Api.Controllers;
using ACommerce.Locations.Api.Controllers;
using ACommerce.Chats.Api.Controllers;
using ACommerce.Notifications.Recipients.Api.Controllers;
using ACommerce.Subscriptions.Api.Controllers;
using ACommerce.Subscriptions.Services;
using ACommerce.Payments.Api.Controllers;
using ACommerce.Payments.Abstractions.Contracts;
using ACommerce.Payments.Noon.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ashare-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Ashare API - Shared Spaces Platform...");

    var builder = WebApplication.CreateBuilder(args);

    // Logging
    builder.Host.UseSerilog();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                // Development: localhost only
                policy.WithOrigins(
                    "https://localhost:5001",
                    "http://localhost:5000",
                    "https://localhost:7001",
                    "http://localhost:7000"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            }
            else
            {
                // Production: allow any origin (MAUI app + web)
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        });
    });

    // Controllers - Auto-discover from all referenced API libraries
    builder.Services.AddControllers()
        .AddApplicationPart(typeof(ProfilesController).Assembly)
        .AddApplicationPart(typeof(VendorsController).Assembly)
        .AddApplicationPart(typeof(ProductsController).Assembly)
        .AddApplicationPart(typeof(ProductListingsController).Assembly)
        .AddApplicationPart(typeof(AttributeDefinitionsController).Assembly)
        .AddApplicationPart(typeof(UnitsController).Assembly)
        .AddApplicationPart(typeof(CurrenciesController).Assembly)
        .AddApplicationPart(typeof(OrdersController).Assembly)
        .AddApplicationPart(typeof(DocumentTypesController).Assembly)
        .AddApplicationPart(typeof(LocationSearchController).Assembly)
        .AddApplicationPart(typeof(ChatsController).Assembly)
        .AddApplicationPart(typeof(ContactPointsController).Assembly)
        .AddApplicationPart(typeof(SubscriptionsController).Assembly)
        .AddApplicationPart(typeof(PaymentsController).Assembly) // Payments
        .AddApplicationPart(typeof(RegistryController).Assembly); // Service Registry & Discovery

    builder.Services.AddEndpointsApiExplorer();

    // Database (SQLite - entities auto-discovered from all ACommerce assemblies)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(
            builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=ashare.db");
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    });

    builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

    // Repository & Unit of Work
    builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
    builder.Services.AddScoped(typeof(IBaseAsyncRepository<>), typeof(BaseAsyncRepository<>));

    // Authentication & Authorization (JWT)
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // User Provider (In-Memory for development - replace with DB implementation for production)
    builder.Services.AddSingleton<IUserProvider, InMemoryUserProvider>();

    // ===== Service Registry & Messaging (Self-Hosting for Development) =====
    // Service Registry (allows other services to register and discover endpoints)
    builder.Services.AddServiceRegistryCore();

    // In-Memory Messaging (pub/sub within same process)
    builder.Services.AddInMemoryMessaging("Ashare");

    // SignalR Messaging Hub (inter-service communication)
    builder.Services.AddMessagingHub();

    // Notifications (InApp via SignalR)
    builder.Services.AddNotificationCore(builder.Configuration);
    builder.Services.AddInAppNotifications();
    builder.Services.AddInMemoryNotificationPublisher();

    // ✅ Notification Messaging Handler (subscribes to notify.commands.send and sends notifications)
    builder.Services.AddNotificationMessaging();

    // ✅ Authentication Event Publisher (يجب أن يكون قبل AddNafathTwoFactor)
    builder.Services.AddScoped<IAuthenticationEventPublisher, MessagingAuthenticationEventPublisher>();

    // ✅ Authentication Messaging Handler (subscribes to auth events and sends notifications)
    builder.Services.AddAuthenticationMessaging(options =>
    {
        options.NotificationChannels = [NotificationChannel.InApp]; // SignalR بدلاً من Email
        options.NotifyOnInitiation = false; // لا نرسل إشعار عند بدء التحقق
    });

    // Two-Factor Authentication (Nafath for Saudi Arabia)
    builder.Services.AddInMemoryTwoFactorSessionStore(); // Session store for 2FA
    builder.Services.AddNafathTwoFactor(builder.Configuration); // Nafath provider

    // CQRS (MediatR + AutoMapper + FluentValidation)
    builder.Services.AddSharedKernelCQRS(AppDomain.CurrentDomain.GetAssemblies());

    // SignalR for Real-time Communication
    // NOTE: IRealtimeHub is bound to NotificationHub for InApp notifications to work
    // ChatHub is still mapped but doesn't use IRealtimeHub service directly
    builder.Services.AddACommerceSignalR<NotificationHub, INotificationClient>();

    // Location Services
    builder.Services.AddACommerceLocations();

    // Chat Services
    builder.Services.AddChatsCore();

    // Product Services
    builder.Services.AddScoped<IProductService, ProductService>();

    // Subscription Services
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

    // Payment Provider (Noon)
    builder.Services.AddNoonPayments(builder.Configuration);

    // Ashare Seed Service
    builder.Services.AddScoped<AshareSeedDataService>();

    // Swagger Documentation
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Ashare API",
            Version = "v1.0.0",
            Description = @"
# Ashare - Shared Spaces Platform API

## About
Ashare is a platform for booking shared spaces:
- Meeting rooms
- Co-working spaces
- Event halls
- Studios
- Commercial spaces
- Co-living spaces

## Architecture
Built using ACommerce libraries with configuration-first approach:

### Spaces = Products + Dynamic Attributes
- **ProductCategory**: Space types (residential, commercial, meeting, etc.)
- **AttributeDefinition**: Space properties (capacity, area, amenities, etc.)
- **Product**: Space details
- **ProductListing**: Owner's space listing with price

### Bookings = Orders with Time Attributes
- **Order**: Booking record
- **OrderItem**: Space + time slot
- **DocumentType/DocumentOperation**: Booking workflow states

### Reviews & Ratings
- Reviews module for space ratings

### Location
- Geographic hierarchy for space locations

### Communication
- Chat: Host-guest messaging
- Notifications: Booking alerts

## Endpoints

### Authentication
- `/api/auth/register` - User registration
- `/api/auth/login` - User login
- `/api/auth/me` - Get current user
- `/api/auth/logout` - Logout
- `/api/auth/refresh` - Refresh token
- `/api/auth/2fa/initiate` - Start 2FA (Nafath/SMS/Email)
- `/api/auth/2fa/verify` - Verify 2FA code
- `/api/auth/2fa/cancel` - Cancel 2FA session
- `/api/nafath/webhook` - Nafath callback webhook

### Resources
- `/api/profiles` - User profiles
- `/api/vendors` - Space owners (hosts)
- `/api/products` - Spaces catalog
- `/api/productlistings` - Owner listings
- `/api/attributedefinitions` - Space properties
- `/api/productcategories` - Space types
- `/api/orders` - Bookings
- `/api/documenttypes` - Workflow configurations
- `/api/locations` - Geographic locations
- `/api/chats` - Messaging
- `/api/contactpoints` - Notification settings
",
            Contact = new()
            {
                Name = "Ashare Team",
                Email = "support@ashare.app"
            }
        });

        // JWT Bearer Authentication in Swagger
        options.AddSecurityDefinition("Bearer", new()
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
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

    // Middleware Pipeline - Swagger always enabled for testing
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ashare API v1.0");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "Ashare API";
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
    });

    // HTTPS redirection only in development (production uses HTTP)
    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // SignalR Hubs
    app.MapHub<ChatHub>("/hubs/chat");
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapMessagingHub(); // /hubs/messaging - Inter-service messaging

    // Database initialization and seeding
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Log.Information("Ensuring database is created...");
        await dbContext.Database.EnsureCreatedAsync();

        // Seed data
        Log.Information("Seeding initial data...");
        var seedService = scope.ServiceProvider.GetRequiredService<AshareSeedDataService>();
        await seedService.SeedAsync();

        Log.Information("Database ready with seed data!");
    }

    // Health check endpoint
    app.MapGet("/health", () => new
    {
        Service = "Ashare API",
        Status = "Running",
        Version = "1.0.0"
    });

    // ✅ تسجيل الخدمة في Service Registry
    var serviceBaseUrl = app.Environment.IsDevelopment()
        ? "https://localhost:5001"
        : "http://safqatasheer-001-site1.qtempurl.com";

    using (var scope = app.Services.CreateScope())
    {
        var registry = scope.ServiceProvider.GetRequiredService<IServiceRegistry>();
        await registry.RegisterAsync(new ServiceRegistration
        {
            ServiceName = "Ashare",
            Version = "v1",
            BaseUrl = serviceBaseUrl,
            Environment = app.Environment.EnvironmentName,
            EnableHealthCheck = true,
            HealthCheckPath = "/health",
            Tags = new Dictionary<string, string>
            {
                ["hubs"] = "/hubs/notifications,/hubs/chat,/hubs/messaging"
            }
        });
        Log.Information("Ashare service registered in Service Registry at {BaseUrl}", serviceBaseUrl);
    }

    Log.Information("Ashare API started successfully!");
    Log.Information("Swagger UI available at: {BaseUrl}", serviceBaseUrl);

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
