using Serilog;
using Microsoft.EntityFrameworkCore;
using ACommerce.SharedKernel.Infrastructure.EFCores.Context;
using ACommerce.SharedKernel.Infrastructure.EFCores.Extensions;
using ACommerce.SharedKernel.CQRS.Extensions;
using ACommerce.SharedKernel.AspNetCore.Extensions;
using ACommerce.Authentication.JWT;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Factories;
using ACommerce.Realtime.SignalR.Hubs;
using ACommerce.Chats.Core.Hubs;
using ACommerce.Realtime.SignalR.Extensions;
using ACommerce.Chats.Core.Extensions;

// ════════════════════════════════════════════════════════════════
// 🎯 ACommerce E-Shop API - Complete E-Commerce Backend
// ════════════════════════════════════════════════════════════════

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/eshop-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("🚀 Starting ACommerce E-Shop API...");

    var builder = WebApplication.CreateBuilder(args);

    // ════════════════════════════════════════════════════════════════
    // 📦 Services Configuration
    // ════════════════════════════════════════════════════════════════

    // Logging
    builder.Host.UseSerilog();
    builder.Services.AddScoped<DbContext, ApplicationDbContext>();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // Controllers & API
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // ════════════════════════════════════════════════════════════════
    // 🗄️ Database Configuration (SQLite for simplicity)
    // ════════════════════════════════════════════════════════════════
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(
            builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=eshop.db");
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    });

    // Repository & Unit of Work
    builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
    builder.Services.AddScoped(typeof(IBaseAsyncRepository<>), typeof(BaseAsyncRepository<>));

    // ════════════════════════════════════════════════════════════════
    // 🔐 Authentication & Authorization (OpenIddict)
    // ════════════════════════════════════════════════════════════════
    builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

    builder.Services.AddJwtAuthentication(options =>
    {
        options.Issuer = builder.Configuration["Authentication:Issuer"]
            ?? "https://localhost:5001";
    });

    // ════════════════════════════════════════════════════════════════
    // 🗺️ AutoMapper & CQRS
    // ════════════════════════════════════════════════════════════════
    // AddSharedKernelCQRS includes AutoMapper, MediatR, and FluentValidation
    builder.Services.AddSharedKernelCQRS(AppDomain.CurrentDomain.GetAssemblies());

    // ════════════════════════════════════════════════════════════════
    // 📡 SignalR for Real-time Communication
    // ════════════════════════════════════════════════════════════════
    builder.Services.AddSignalR();
    builder.Services.AddACommerceSignalR<ChatHub, IChatClient>();

    // ════════════════════════════════════════════════════════════════
    // 💬 Chat Services
    // ════════════════════════════════════════════════════════════════
    builder.Services.AddChatServices();

    // ════════════════════════════════════════════════════════════════
    // 📝 Swagger Documentation
    // ════════════════════════════════════════════════════════════════
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "ACommerce E-Shop API",
            Version = "v2.0.0",
            Description = @"
# 🛒 Complete E-Commerce Backend API

## ✨ Features:
- 👤 **Authentication & Authorization** (OpenIddict + JWT)
- 📦 **Product Catalog** (Attributes, Units, Currencies, Products)
- 🛍️ **Shopping Cart & Orders**
- 💳 **Payment Processing**
- 🚚 **Shipping Management**
- 👥 **Vendor Marketplace**
- 💬 **Chat & Real-time Notifications**
- 📧 **Contact Points Management**
- 📊 **User Profiles**

## 🏗️ Architecture:
- Clean Architecture + DDD
- CQRS with MediatR
- Repository Pattern
- Separation of Concerns
- Independent Domain Systems

## 🔧 Technologies:
- .NET 9.0
- Entity Framework Core
- OpenIddict
- SignalR
- AutoMapper
- FluentValidation
",
            Contact = new()
            {
                Name = "ACommerce Team",
                Email = "support@acommerce.com"
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
            Description = "JWT Authorization header using the Bearer scheme. Enter your token below."
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

    // ════════════════════════════════════════════════════════════════
    // 🏗️ Build Application
    // ════════════════════════════════════════════════════════════════
    var app = builder.Build();

    // ════════════════════════════════════════════════════════════════
    // 🔧 Middleware Pipeline
    // ════════════════════════════════════════════════════════════════

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ACommerce E-Shop API v2.0");
            options.RoutePrefix = string.Empty; // Swagger at root
            options.DocumentTitle = "ACommerce E-Shop API";
            options.EnableDeepLinking();
            options.DisplayRequestDuration();
        });
    }

    app.UseHttpsRedirection();
    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();

    // SignalR Hubs
    app.MapHub<ChatHub>("/hubs/chat");
    app.MapHub<NotificationHub>("/hubs/notifications");

    // ════════════════════════════════════════════════════════════════
    // 🗄️ Database Migration & Seeding
    // ════════════════════════════════════════════════════════════════
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Log.Information("📊 Ensuring database is created...");
        await dbContext.Database.EnsureCreatedAsync();

        Log.Information("✅ Database ready!");
    }

    // ════════════════════════════════════════════════════════════════
    // 🚀 Run Application
    // ════════════════════════════════════════════════════════════════
    Log.Information("✅ ACommerce E-Shop API started successfully!");
    Log.Information("📖 Swagger UI available at: https://localhost:5001");
    Log.Information("🌐 API Base URL: https://localhost:5001/api");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
