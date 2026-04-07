using ACommerce.Client.Http;
using ACommerce.Client.Http.Extensions;
using ACommerce.Client.Operations;
using ACommerce.OperationEngine.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ACommerce.Client.Maui;

/// <summary>
/// امتدادات MAUI / MAUI Blazor لتسجيل عميل عشير كاملاً بسطر واحد.
///
/// الاستخدام في MauiProgram.cs:
///   builder.Services.AddAshareClient(options =>
///   {
///       options.BaseAddress = "https://api.ashare.app";
///   }, routes =>
///   {
///       routes.Map("listing.create", HttpMethod.Post, "/api/listings");
///       routes.Map("auth.sms.request", HttpMethod.Post, "/api/auth/sms/request");
///       // ... كل نوع عملية مع URL
///   });
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAshareClient(
        this IServiceCollection services,
        Action<AshareClientOptions> configure,
        Action<HttpRouteRegistry>? routes = null)
    {
        var options = new AshareClientOptions { BaseAddress = "http://localhost:5500" };
        configure(options);
        services.AddSingleton(options);

        // OperationEngine على العميل
        services.AddSingleton<OpEngine>(sp =>
            new OpEngine(sp, sp.GetRequiredService<ILogger<OpEngine>>()));

        // HttpClient المحاسبي
        services.AddHttpClient("ashare", client =>
        {
            client.BaseAddress = new Uri(options.BaseAddress);
            client.Timeout = options.Timeout;
            if (!string.IsNullOrEmpty(options.BearerToken))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.BearerToken);
        });

        services.AddSingleton<HttpClient>(sp =>
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("ashare"));

        // HttpDispatcher + routes
        services.AddAshareHttpDispatcher(
            opt =>
            {
                opt.ClientPartyId = options.ClientPartyId;
                opt.ServerPartyId = options.ServerPartyId ?? $"Server:{new Uri(options.BaseAddress).Host}";
            },
            routes);

        // ClientOpEngine (طبقة التزام الأوامر)
        services.AddSingleton<ClientOpEngine>();

        return services;
    }
}
