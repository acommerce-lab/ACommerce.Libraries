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

// Force load assemblies for CQRS auto-discovery
using ACommerce.Catalog.Attributes.Entities;
using ACommerce.Catalog.Attributes.Enums;

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
using ACommerce.Files.Storage.GoogleCloud.Extensions;
using Ashare.Api.Middleware;
using Microsoft.AspNetCore.SignalR;

// Admin Controllers
using ACommerce.Admin.Dashboard.Api.Controllers;
using ACommerce.Admin.Orders.Api.Controllers;
using ACommerce.Admin.Listings.Api.Controllers;
using ACommerce.Admin.Reports.Api.Controllers;
using ACommerce.Admin.AuditLog.Api.Controllers;
using ACommerce.Admin.Dashboard;
using ACommerce.Admin.Orders;
using ACommerce.Admin.Listings;
using ACommerce.Admin.Reports;
using ACommerce.Admin.AuditLog;

// Legal Pages
using ACommerce.LegalPages.Api.Controllers;
using ACommerce.LegalPages.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ashare-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Force load assemblies before GetAssemblies() is called
// This ensures all referenced assemblies are loaded for CQRS auto-discovery
_ = typeof(AttributeDefinition).Assembly;
_ = typeof(AttributeType).Assembly;
_ = typeof(ACommerce.LegalPages.Entities.LegalPage).Assembly;

// ✅ تسجيل الـ Entities مبكراً قبل بناء DbContext
// هذا ضروري لأن EntityDiscoveryRegistry يجب أن يحتوي على جميع الـ Entities
// قبل أول استدعاء لـ ApplicationDbContext.OnModelCreating
ACommerce.SharedKernel.Abstractions.Entities.EntityDiscoveryRegistry.RegisterEntity<ACommerce.LegalPages.Entities.LegalPage>();

try
{
    Log.Information("Starting Ashare API - Shared Spaces Platform...");

    var builder = WebApplication.CreateBuilder(args);

    // Cloud Run: Use PORT environment variable
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    Log.Information("🚀 Listening on port {Port}", port);

    // Logging
    builder.Host.UseSerilog();

    // Memory Cache (required by ACommerce libraries)
    builder.Services.AddMemoryCache();

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
        .AddApplicationPart(typeof(RegistryController).Assembly) // Service Registry & Discovery
        .AddApplicationPart(typeof(DashboardController).Assembly) // Admin Dashboard
        .AddApplicationPart(typeof(AdminOrdersController).Assembly) // Admin Orders
        .AddApplicationPart(typeof(AdminListingsController).Assembly) // Admin Listings
        .AddApplicationPart(typeof(ReportsController).Assembly) // Admin Reports
        .AddApplicationPart(typeof(AuditLogController).Assembly) // Admin Audit Log
        .AddApplicationPart(typeof(LegalPagesController).Assembly); // Legal Pages

    builder.Services.AddEndpointsApiExplorer();

    // Database - Auto-detect provider from connection string
    // First check environment variable, then fall back to config
    var connectionString = Environment.GetEnvironmentVariable("GOOGLE_SQL_CONNECTION_STRING") 
        ?? builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("Data Source="))
        {
            // SQLite for local development
            Log.Information("Using SQLite database");
            options.UseSqlite(connectionString ?? "Data Source=ashare.db");
        }
        else
        {
            // SQL Server for Azure/Production
            Log.Information("Using SQL Server database");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        }
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

    // ✅ SignalR Exception Filter - لمنع انهيار الـ Hubs عند حدوث أخطاء
    builder.Services.AddSingleton<SignalRExceptionFilter>();
    builder.Services.AddSignalR(options =>
    {
        options.AddFilter<SignalRExceptionFilter>();
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    });

    // Location Services
    builder.Services.AddACommerceLocations();

    // Chat Services
    builder.Services.AddChatsCore();

    // Product Services
    builder.Services.AddScoped<IProductService, ProductService>();

    // Admin Services
    builder.Services.AddAdminDashboard();
    builder.Services.AddAdminOrders();
    builder.Services.AddAdminListings();
    builder.Services.AddAdminReports();
    builder.Services.AddAdminAuditLog();

    // Legal Pages
    builder.Services.AddLegalPages();

    // Subscription Services
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

    // Google Cloud Storage (for image uploads)
    builder.Services.AddGoogleCloudStorage(builder.Configuration);

    // Payment Provider (Noon) - مع استخدام HostSettings لبناء ReturnUrl تلقائياً
    var webBaseUrlForPayment = builder.Configuration["HostSettings:WebBaseUrl"] 
        ?? builder.Configuration["HostSettings:BaseUrl"]
        ?? "https://ashare.app";
    
    builder.Services.AddNoonPayments(builder.Configuration);
    builder.Services.PostConfigure<ACommerce.Payments.Noon.Models.NoonOptions>(options =>
    {
        if (string.IsNullOrEmpty(options.ReturnUrl))
        {
            options.ReturnUrl = $"{webBaseUrlForPayment.TrimEnd('/')}/host/payment/callback";
            Log.Information("💳 Noon ReturnUrl set from HostSettings: {ReturnUrl}", options.ReturnUrl);
        }
    });

    // Ashare Seed Service
    builder.Services.AddScoped<AshareSeedDataService>();

    // Cache Warm-up Service - DISABLED for debugging
    // builder.Services.AddHostedService<CacheWarmupService>();

    // Swagger Documentation
    builder.Services.AddSwaggerGen(options =>
    {
        // Fix duplicate schema names by using full type name
        options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

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

    // ✅ Global Exception Handler - أول middleware لالتقاط جميع الأخطاء
    app.UseGlobalExceptionHandler();

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

    // Database initialization and seeding (مع حماية من الأخطاء)
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Log.Information("Ensuring database is created...");
        await dbContext.Database.EnsureCreatedAsync();

        // إنشاء الفهارس إذا لم تكن موجودة (مهم للأداء)
        Log.Information("Ensuring indexes exist...");
        await EnsureIndexesAsync(dbContext);

        // Seed data (يمكن تعطيله عبر متغير البيئة SKIP_SEEDING=true)
        var skipSeeding = Environment.GetEnvironmentVariable("SKIP_SEEDING")?.ToLower() == "true";
        if (!skipSeeding)
        {
            Log.Information("Seeding initial data...");
            var seedService = scope.ServiceProvider.GetRequiredService<AshareSeedDataService>();
            await seedService.SeedAsync();
        }
        else
        {
            Log.Information("⏭️ Skipping seed data (SKIP_SEEDING=true)");
        }

        Log.Information("Database ready with seed data!");

        // Warm-up: Pre-heat EF Core and AutoMapper to avoid cold start delays
        Log.Information("Warming up EF Core and AutoMapper...");
        var warmupStopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Warm up EF Core by executing a simple query
        _ = await dbContext.Set<ACommerce.Catalog.Listings.Entities.ProductListing>()
            .AsNoTracking()
            .Take(1)
            .ToListAsync();
        
        // Warm up AutoMapper by resolving the mapper
        var mapper = scope.ServiceProvider.GetRequiredService<AutoMapper.IMapper>();
        
        warmupStopwatch.Stop();
        Log.Information("Warm-up completed in {ElapsedMs}ms", warmupStopwatch.ElapsedMilliseconds);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Database initialization failed - continuing anyway");
        // لا نوقف التطبيق - نستمر بدون قاعدة البيانات
    }

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new
    {
        Service = "Ashare API",
        Status = "Healthy",
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow
    }));

    // Readiness check (for Kubernetes/Cloud Run)
    app.MapGet("/ready", async (ApplicationDbContext db) =>
    {
        try
        {
            // Check database connectivity
            await db.Database.CanConnectAsync();
            return Results.Ok(new { Status = "Ready" });
        }
        catch
        {
            return Results.StatusCode(503);
        }
    });

    // ✅ قراءة HostSettings (مكان واحد لجميع الروابط)
    var hostBaseUrl = builder.Configuration["HostSettings:BaseUrl"]
        ?? Environment.GetEnvironmentVariable("SERVICE_URL")
        ?? (app.Environment.IsDevelopment() ? "http://localhost:3000" : "https://ashareapi-hygabpf3ajfmevfs.canadaeast-01.azurewebsites.net");
    
    var webBaseUrl = builder.Configuration["HostSettings:WebBaseUrl"] ?? hostBaseUrl;
    
    Log.Information("🌐 Host URLs configured: API={ApiUrl}, Web={WebUrl}", hostBaseUrl, webBaseUrl);
    
    var serviceBaseUrl = hostBaseUrl;

    try
    {
        using var scope = app.Services.CreateScope();
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
        
        // تسجيل الخدمة أيضاً باسم Marketplace للتوافق مع Client SDKs
        await registry.RegisterAsync(new ServiceRegistration
        {
            ServiceName = "Marketplace",
            Version = "v1",
            BaseUrl = serviceBaseUrl,
            Environment = app.Environment.EnvironmentName,
            EnableHealthCheck = true,
            HealthCheckPath = "/health"
        });

        // تسجيل خدمة الدفع (Payments) للتوافق مع Client SDKs
        await registry.RegisterAsync(new ServiceRegistration
        {
            ServiceName = "Payments",
            Version = "v1",
            BaseUrl = serviceBaseUrl,
            Environment = app.Environment.EnvironmentName,
            EnableHealthCheck = true,
            HealthCheckPath = "/health"
        });
        Log.Information("Ashare/Marketplace/Payments services registered in Service Registry at {BaseUrl}", serviceBaseUrl);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "⚠️ Service Registry registration failed - continuing anyway");
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

// دالة إنشاء الفهارس للأداء الأمثل - تعمل فقط مع SQLite
static async Task EnsureIndexesAsync(ApplicationDbContext dbContext)
{
    // هذه الأوامر تعمل فقط مع SQLite - تخطي SQL Server و PostgreSQL
    var providerName = dbContext.Database.ProviderName ?? "";
    if (!providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        Log.Information("Skipping index creation - not SQLite database (provider: {Provider})", providerName);
        return;
    }

    var indexCommands = new[]
    {
        // فهارس جدول ProductListings
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_IsDeleted\" ON \"ProductListings\" (\"IsDeleted\")",
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_IsActive\" ON \"ProductListings\" (\"IsActive\")",
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_Status\" ON \"ProductListings\" (\"Status\")",
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_VendorId\" ON \"ProductListings\" (\"VendorId\")",
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_ProductId\" ON \"ProductListings\" (\"ProductId\")",
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_CategoryId\" ON \"ProductListings\" (\"CategoryId\")",
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_CreatedAt\" ON \"ProductListings\" (\"CreatedAt\")",
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_ViewCount\" ON \"ProductListings\" (\"ViewCount\")",
        // فهارس مركبة للاستعلامات الشائعة
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_Composite_Status\" ON \"ProductListings\" (\"IsDeleted\", \"IsActive\", \"Status\", \"CreatedAt\")",
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_Composite_Views\" ON \"ProductListings\" (\"IsDeleted\", \"IsActive\", \"Status\", \"ViewCount\")",
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_Composite_Vendor\" ON \"ProductListings\" (\"VendorId\", \"IsDeleted\", \"IsActive\", \"CreatedAt\")",
        "CREATE INDEX IF NOT EXISTS \"IX_ProductListings_Composite_Category\" ON \"ProductListings\" (\"CategoryId\", \"IsDeleted\", \"IsActive\", \"Status\", \"CreatedAt\")",
        
        // فهارس جدول AttributeValues (للبحث السريع في السمات)
        "CREATE INDEX IF NOT EXISTS \"IX_AttributeValues_EntityId\" ON \"AttributeValues\" (\"EntityId\")",
        "CREATE INDEX IF NOT EXISTS \"IX_AttributeValues_AttributeDefinitionId\" ON \"AttributeValues\" (\"AttributeDefinitionId\")",
        "CREATE INDEX IF NOT EXISTS \"IX_AttributeValues_IsDeleted\" ON \"AttributeValues\" (\"IsDeleted\")",
        "CREATE INDEX IF NOT EXISTS \"IX_AttributeValues_Composite\" ON \"AttributeValues\" (\"EntityId\", \"IsDeleted\")",
        
        // فهارس جدول Products
        "CREATE INDEX IF NOT EXISTS \"IX_Products_IsDeleted\" ON \"Products\" (\"IsDeleted\")",
        "CREATE INDEX IF NOT EXISTS \"IX_Products_CategoryId\" ON \"Products\" (\"CategoryId\")",
        "CREATE INDEX IF NOT EXISTS \"IX_Products_CreatedAt\" ON \"Products\" (\"CreatedAt\")",
        
        // فهارس جدول ProductCategories
        "CREATE INDEX IF NOT EXISTS \"IX_ProductCategories_IsDeleted\" ON \"ProductCategories\" (\"IsDeleted\")",
        "CREATE INDEX IF NOT EXISTS \"IX_ProductCategories_ParentId\" ON \"ProductCategories\" (\"ParentId\")",
        
        // فهارس جدول Vendors
        "CREATE INDEX IF NOT EXISTS \"IX_Vendors_IsDeleted\" ON \"Vendors\" (\"IsDeleted\")",
        "CREATE INDEX IF NOT EXISTS \"IX_Vendors_ProfileId\" ON \"Vendors\" (\"ProfileId\")"
    };

    var successCount = 0;
    var skipCount = 0;
    
    foreach (var command in indexCommands)
    {
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync(command);
            successCount++;
        }
        catch (Exception ex)
        {
            // قد يفشل إذا الجدول غير موجود بعد - هذا طبيعي
            if (!ex.Message.Contains("no such table") && !ex.Message.Contains("does not exist"))
            {
                Log.Warning("Index creation warning: {Message}", ex.Message);
            }
            skipCount++;
        }
    }
    
    Log.Information("Database indexes: {Success} created, {Skipped} skipped", successCount, skipCount);
}
