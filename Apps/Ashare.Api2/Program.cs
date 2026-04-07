using ACommerce.Authentication.Operations;
using ACommerce.Authentication.Operations.Abstractions;
using ACommerce.Authentication.Providers.Token;
using ACommerce.Authentication.Providers.Token.Extensions;
using ACommerce.Authentication.TwoFactor.Operations;
using ACommerce.Authentication.TwoFactor.Operations.Abstractions;
using ACommerce.Authentication.TwoFactor.Providers.Email.Extensions;
using ACommerce.Authentication.TwoFactor.Providers.Sms.Extensions;
using ACommerce.Notification.Operations.Abstractions;
using ACommerce.Notification.Providers.InApp.Extensions;
using ACommerce.OperationEngine.Core;
using ACommerce.Payments.Operations;
using ACommerce.Payments.Operations.Abstractions;
using ACommerce.Payments.Providers.Noon;
using ACommerce.Payments.Providers.Noon.Options;
using ACommerce.Realtime.Operations.Abstractions;
using ACommerce.Realtime.Providers.InMemory.Extensions;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Infrastructure.EFCores.Extensions;
using ACommerce.SharedKernel.Abstractions.Repositories;
using Ashare.Api2.Entities;
using Ashare.Api2.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────
// Services
// ─────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// تسجيل كيانات Ashare.Api2 في الـ Auto-Discovery Registry
EntityDiscoveryRegistry.RegisterEntity(typeof(User));
EntityDiscoveryRegistry.RegisterEntity(typeof(Category));
EntityDiscoveryRegistry.RegisterEntity(typeof(Listing));
EntityDiscoveryRegistry.RegisterEntity(typeof(Booking));
EntityDiscoveryRegistry.RegisterEntity(typeof(Ashare.Api2.Entities.Payment));
EntityDiscoveryRegistry.RegisterEntity(typeof(Ashare.Api2.Entities.Notification));
EntityDiscoveryRegistry.RegisterEntity(typeof(Conversation));
EntityDiscoveryRegistry.RegisterEntity(typeof(Message));
EntityDiscoveryRegistry.RegisterEntity(typeof(DeviceToken));
EntityDiscoveryRegistry.RegisterEntity(typeof(TwoFactorChallengeRecord));

// EF Core (InMemory) + Repository Factory
builder.Services.AddACommerceInMemoryDatabase("AshareApi2Db");

// === OperationEngine النواة ===
builder.Services.AddSingleton<OpEngine>(sp =>
    new OpEngine(sp, sp.GetRequiredService<ILogger<OpEngine>>()));

// === Realtime: InMemory transport (للتطوير) ===
builder.Services.AddInMemoryRealtimeTransport();

// === Notifications: InApp مبنية على IRealtimeTransport ===
builder.Services.AddInAppNotificationChannel(opt =>
{
    opt.MethodName = "ReceiveNotification";
    opt.AllowOffline = true;
});

// === Authentication: Token provider ===
// نسجل AshareTokenStore كـ ITokenValidator + ITokenIssuer
builder.Services.AddSingleton<AshareTokenStore>();
builder.Services.AddSingleton<ITokenValidator>(sp => sp.GetRequiredService<AshareTokenStore>());
builder.Services.AddSingleton<ITokenIssuer>(sp => sp.GetRequiredService<AshareTokenStore>());
builder.Services.AddTokenAuthenticator();

// AuthConfig + AuthService
builder.Services.AddSingleton<AuthConfig>(sp =>
{
    var config = new AuthConfig();
    config.AddAuthenticator(sp.GetRequiredService<TokenAuthenticator>());
    config.UseIssuer(sp.GetRequiredService<ITokenIssuer>());
    return config;
});
builder.Services.AddSingleton<AuthService>();

// === 2FA: SMS + Email (test/dev مع logging senders) ===
builder.Services.AddSmsTwoFactor();
builder.Services.AddEmailTwoFactor(opt =>
{
    opt.Subject = "رمز التحقق - عشير";
    opt.BodyTemplate = "رمز التحقق الخاص بك في عشير: {0}\nصالح لمدة 10 دقائق.";
});

// TwoFactorConfig + TwoFactorService
builder.Services.AddSingleton<TwoFactorConfig>(sp =>
{
    var cfg = new TwoFactorConfig();
    foreach (var ch in sp.GetServices<ITwoFactorChannel>())
        cfg.AddChannel(ch);
    return cfg;
});
builder.Services.AddSingleton<TwoFactorService>();

// === Payments: Noon provider ===
builder.Services.AddSingleton<NoonOptions>(new NoonOptions
{
    BusinessIdentifier = "ashare-business",
    ApplicationIdentifier = "ashare-app",
    ApiKey = "test-key-placeholder",
    Mode = NoonMode.Test,
    DefaultCurrency = "SAR",
    Channel = "Web"
});
builder.Services.AddHttpClient<NoonPaymentGateway>();
builder.Services.AddSingleton<IPaymentGateway>(sp => sp.GetRequiredService<NoonPaymentGateway>());

builder.Services.AddSingleton<PaymentConfig>(sp =>
{
    var cfg = new PaymentConfig();
    foreach (var gw in sp.GetServices<IPaymentGateway>())
        cfg.AddGateway(gw);
    return cfg;
});
builder.Services.AddSingleton<PaymentService>();

// === Seeder ===
builder.Services.AddScoped<AshareSeeder>();

// ─────────────────────────────────────────────────────────
// Build app
// ─────────────────────────────────────────────────────────
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "Ashare.Api2",
    description = "خدمة عشير الخلفية المبنية على OperationEngine المحاسبي",
    version = "1.0.0",
    docs = "/swagger"
}));

app.MapGet("/health", () => Results.Ok(new { status = "healthy", time = DateTime.UtcNow }));

// === تشغيل البذر ===
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<AshareSeeder>();
    await seeder.SeedAsync();
}

app.Run();
