using Serilog;
using Microsoft.EntityFrameworkCore;
using ACommerce.SharedKernel.Infrastructure.EFCores.Context;
using ACommerce.SharedKernel.CQRS.Extensions;
using ACommerce.Authentication.JWT;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Factories;
using ACommerce.Realtime.SignalR.Hubs;
using ACommerce.Realtime.SignalR.Extensions;
using ACommerce.Chats.Core.Hubs;
using ACommerce.Chats.Core.Extensions;

// ════════════════════════════════════════════════════════════════
// 🏠 عشير API - منصة حجز المساحات المشتركة
// ════════════════════════════════════════════════════════════════

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ashare-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("🚀 Starting Ashare API - Shared Spaces Platform...");

    var builder = WebApplication.CreateBuilder(args);

    // ════════════════════════════════════════════════════════════════
    // 📦 Services Configuration
    // ════════════════════════════════════════════════════════════════

    // Logging
    builder.Host.UseSerilog();
    builder.Services.AddScoped<DbContext, ApplicationDbContext>();

    // CORS - Allow mobile apps and web clients
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.SetIsOriginAllowed(_ => true) // Allow all origins for mobile apps
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // Controllers & API
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // ════════════════════════════════════════════════════════════════
    // 🗄️ Database Configuration (SQLite for development)
    // ════════════════════════════════════════════════════════════════
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(
            builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=ashare.db");
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    });

    // Register DbContext base type for repositories
    builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

    // Repository & Unit of Work
    builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
    builder.Services.AddScoped(typeof(IBaseAsyncRepository<>), typeof(BaseAsyncRepository<>));

    // ════════════════════════════════════════════════════════════════
    // 🔐 Authentication & Authorization (JWT + Nafath)
    // ════════════════════════════════════════════════════════════════
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // ════════════════════════════════════════════════════════════════
    // 🗺️ AutoMapper & CQRS
    // ════════════════════════════════════════════════════════════════
    builder.Services.AddSharedKernelCQRS(AppDomain.CurrentDomain.GetAssemblies());

    // ════════════════════════════════════════════════════════════════
    // 📡 SignalR for Real-time Communication
    // ════════════════════════════════════════════════════════════════
    builder.Services.AddACommerceSignalR<ChatHub, IChatClient>();

    // ════════════════════════════════════════════════════════════════
    // 💬 Chat Services
    // ════════════════════════════════════════════════════════════════
    builder.Services.AddChatsCore();

    // ════════════════════════════════════════════════════════════════
    // 📝 Swagger Documentation
    // ════════════════════════════════════════════════════════════════
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "عشير API - Ashare",
            Version = "v1.0.0",
            Description = @"
# 🏠 عشير - منصة حجز المساحات المشتركة

## ✨ الميزات:
- 🔐 **المصادقة** (نفاذ + JWT)
- 🏢 **إدارة المساحات** (قاعات اجتماعات، مساحات عمل مشتركة، قاعات فعاليات، استوديوهات)
- 📅 **نظام الحجوزات** (حجز فوري، إدارة المواعيد)
- ⭐ **التقييمات والمراجعات**
- 💬 **المحادثات والدردشة**
- 🔔 **الإشعارات** (Firebase + داخل التطبيق)
- 📍 **المواقع الجغرافية**
- 👤 **الملفات الشخصية**

## 🏗️ البنية:
- Clean Architecture + DDD
- CQRS with MediatR
- Repository Pattern
- SignalR for Real-time

## 📱 التطبيقات:
- Android
- iOS
- Windows

## 🌍 اللغات المدعومة:
- العربية
- English
- اردو
",
            Contact = new()
            {
                Name = "عشير",
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
            Description = "أدخل رمز JWT الخاص بك"
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
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ashare API v1.0");
            options.RoutePrefix = string.Empty;
            options.DocumentTitle = "عشير API";
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
    Log.Information("✅ Ashare API started successfully!");
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
