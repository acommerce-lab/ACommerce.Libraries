using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using ACommerce.SharedKernel.Infrastructure.EFCores.Context;
using ACommerce.SharedKernel.Infrastructure.EFCores.Extensions;
using ACommerce.SharedKernel.CQRS.Extensions;
using ACommerce.Authentication.JWT;
using ACommerce.Authentication.Users.Abstractions;
using ACommerce.Authentication.TwoFactor.SessionStore.Redis;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Factories;
using ACommerce.Catalog.Products.Services;
using ACommerce.Subscriptions.Services;
using ACommerce.Files.Storage.GoogleCloud.Extensions;
using ACommerce.Messaging.InMemory.Extensions;
using ACommerce.Marketplace.GCP.Infrastructure;
using ACommerce.Marketplace.GCP.Services;
using ACommerce.Marketplace.GCP.Configuration;

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
using ACommerce.Subscriptions.Api.Controllers;
using ACommerce.Payments.Api.Controllers;

// Force load assemblies for CQRS auto-discovery
using ACommerce.Catalog.Attributes.Entities;
using ACommerce.Catalog.Attributes.Enums;

// ============================================================================
// ACommerce Marketplace API - Google Cloud Platform Edition
// Optimized for GCP Marketplace deployment with usage-based billing support
// ============================================================================

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

// Force load assemblies before GetAssemblies() is called
_ = typeof(AttributeDefinition).Assembly;
_ = typeof(AttributeType).Assembly;

try
{
    Log.Information("Starting ACommerce Marketplace API (GCP Edition)...");

    var builder = WebApplication.CreateBuilder(args);

    // =========================================================================
    // CLOUD CONFIGURATION - All settings via Environment Variables
    // =========================================================================

    // Port configuration (Cloud Run injects PORT)
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    Log.Information("Listening on port {Port}", port);

    // Configure Serilog for GCP Cloud Logging
    // Note: Cloud Run automatically sends stdout/stderr to Cloud Logging
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", "acommerce-marketplace")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

        // Use JSON format in production for better Cloud Logging parsing
        var gcpProjectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID");
        if (!string.IsNullOrEmpty(gcpProjectId) && !context.HostingEnvironment.IsDevelopment())
        {
            // Structured JSON logging for Cloud Logging
            configuration.WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter());
        }
        else
        {
            // Human-readable format for development
            configuration.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        }
    });

    // Memory Cache
    builder.Services.AddMemoryCache();

    // CORS - Allow any for cloud deployment
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
            if (!string.IsNullOrEmpty(allowedOrigins))
            {
                policy.WithOrigins(allowedOrigins.Split(','))
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
            else
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        });
    });

    // =========================================================================
    // CONTROLLERS - Auto-discover from all referenced API libraries
    // =========================================================================
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
        .AddApplicationPart(typeof(SubscriptionsController).Assembly)
        .AddApplicationPart(typeof(PaymentsController).Assembly);

    builder.Services.AddEndpointsApiExplorer();

    // =========================================================================
    // DATABASE - Flexible provider selection via environment variables
    // Supports: PostgreSQL (Cloud SQL), SQL Server, SQLite (dev)
    // =========================================================================
    var dbConfig = DatabaseConfiguration.FromEnvironment();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        switch (dbConfig.Provider)
        {
            case DatabaseProvider.PostgreSQL:
                Log.Information("Using PostgreSQL database");
                options.UseNpgsql(dbConfig.ConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);

                    // Cloud SQL Proxy uses Unix socket
                    if (!string.IsNullOrEmpty(dbConfig.UnixSocketPath))
                    {
                        // Connection string already includes socket path
                    }
                });
                break;

            case DatabaseProvider.SqlServer:
                Log.Information("Using SQL Server database");
                options.UseSqlServer(dbConfig.ConnectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
                break;

            case DatabaseProvider.SQLite:
            default:
                Log.Information("Using SQLite database (development mode)");
                options.UseSqlite(dbConfig.ConnectionString);
                break;
        }

        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    });

    builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

    // Repository & Unit of Work
    builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
    builder.Services.AddScoped(typeof(IBaseAsyncRepository<>), typeof(BaseAsyncRepository<>));

    // =========================================================================
    // AUTHENTICATION - JWT with Redis session store for cloud
    // =========================================================================
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddSingleton<IUserProvider, InMemoryUserProvider>();

    // Redis for session management (required in production)
    var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
        ?? builder.Configuration["Redis:ConnectionString"];

    if (!string.IsNullOrEmpty(redisConnection) && redisConnection != "localhost:6379")
    {
        Log.Information("Using Redis for session store");
        builder.Services.AddRedisTwoFactorSessionStore(builder.Configuration);
    }
    else
    {
        Log.Warning("Redis not configured - using in-memory session store (not recommended for production)");
    }

    // =========================================================================
    // MESSAGING - In-Memory for single instance, can be replaced with Pub/Sub
    // =========================================================================
    builder.Services.AddInMemoryMessaging("ACommerceMarketplace");

    // =========================================================================
    // CQRS (MediatR + AutoMapper + FluentValidation)
    // =========================================================================
    builder.Services.AddSharedKernelCQRS(AppDomain.CurrentDomain.GetAssemblies());

    // =========================================================================
    // SERVICES
    // =========================================================================
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

    // Google Cloud Storage
    builder.Services.AddGoogleCloudStorage(builder.Configuration);

    // =========================================================================
    // GCP USAGE REPORTING - Central hook for billing integration
    // =========================================================================
    builder.Services.AddSingleton<IUsageReportingService, GcpUsageReportingService>();
    builder.Services.Configure<GcpBillingOptions>(options =>
    {
        options.ProjectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID") ?? "";
        options.ServiceName = Environment.GetEnvironmentVariable("GCP_SERVICE_NAME") ?? "acommerce-marketplace";
        options.EntitlementId = Environment.GetEnvironmentVariable("GCP_ENTITLEMENT_ID") ?? "";
        options.ConsumerId = Environment.GetEnvironmentVariable("GCP_CONSUMER_ID") ?? "";
        options.EnableUsageReporting = Environment.GetEnvironmentVariable("GCP_ENABLE_USAGE_REPORTING")?.ToLower() == "true";
    });

    // =========================================================================
    // HEALTH CHECKS - Comprehensive checks for cloud deployment
    // =========================================================================
    var healthChecksBuilder = builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

    // Database health check based on provider
    switch (dbConfig.Provider)
    {
        case DatabaseProvider.PostgreSQL:
            healthChecksBuilder.AddNpgSql(
                dbConfig.ConnectionString,
                name: "database",
                tags: ["ready", "db"]);
            break;
        case DatabaseProvider.SqlServer:
            healthChecksBuilder.AddSqlServer(
                dbConfig.ConnectionString,
                name: "database",
                tags: ["ready", "db"]);
            break;
    }

    // Redis health check if configured
    if (!string.IsNullOrEmpty(redisConnection) && redisConnection != "localhost:6379")
    {
        healthChecksBuilder.AddRedis(
            redisConnection,
            name: "redis",
            tags: ["ready", "cache"]);
    }

    // =========================================================================
    // SWAGGER
    // =========================================================================
    builder.Services.AddSwaggerGen(options =>
    {
        options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

        options.SwaggerDoc("v1", new()
        {
            Title = "ACommerce Marketplace API",
            Version = "v1.0.0",
            Description = @"
# ACommerce Marketplace API - Google Cloud Edition

## Overview
Complete e-commerce marketplace API optimized for Google Cloud Platform deployment.

## Features
- **Authentication**: JWT-based with optional 2FA
- **Catalog**: Products, Categories, Attributes, Units, Currencies
- **Marketplace**: Multi-vendor support, Listings, Subscriptions
- **Sales**: Orders, Cart, Payments
- **Files**: Google Cloud Storage integration

## Environment Variables
All configuration is done via environment variables for cloud deployment:

### Required
- `DATABASE_URL` or `DATABASE_CONNECTION_STRING` - Database connection
- `JWT__SecretKey` - JWT signing key (min 32 chars)

### Optional
- `DATABASE_PROVIDER` - `postgresql`, `sqlserver`, or `sqlite`
- `REDIS_CONNECTION_STRING` - Redis for sessions
- `GCP_PROJECT_ID` - Google Cloud project ID
- `GCP_ENABLE_USAGE_REPORTING` - Enable billing integration
- `ALLOWED_ORIGINS` - Comma-separated CORS origins

## Health Endpoints
- `/health` - Liveness probe
- `/health/ready` - Readiness probe (includes DB check)
",
            Contact = new()
            {
                Name = "ACommerce Team",
                Email = "support@acommerce.io"
            }
        });

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

    // =========================================================================
    // BUILD APP
    // =========================================================================
    var app = builder.Build();

    // Swagger (always enabled for Marketplace)
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ACommerce Marketplace API v1.0");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "ACommerce Marketplace API";
    });

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // =========================================================================
    // HEALTH CHECK ENDPOINTS
    // =========================================================================

    // Liveness probe - Is the service running?
    app.MapHealthChecks("/health", new()
    {
        Predicate = check => check.Tags.Contains("live"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Readiness probe - Is the service ready to accept traffic?
    app.MapHealthChecks("/health/ready", new()
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Startup probe - Has the service finished initialization?
    app.MapHealthChecks("/health/startup", new()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Note: /healthz endpoint is handled by HealthzController

    // =========================================================================
    // DATABASE INITIALIZATION
    // =========================================================================
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Log.Information("Ensuring database is created...");
        await dbContext.Database.EnsureCreatedAsync();

        // Run migrations if configured
        var runMigrations = Environment.GetEnvironmentVariable("RUN_MIGRATIONS")?.ToLower() == "true";
        if (runMigrations)
        {
            Log.Information("Running database migrations...");
            await dbContext.Database.MigrateAsync();
        }

        Log.Information("Database ready!");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database initialization failed - service may not function correctly");
        // Don't fail startup - let health checks report the issue
    }

    Log.Information("ACommerce Marketplace API started successfully on port {Port}", port);

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}
