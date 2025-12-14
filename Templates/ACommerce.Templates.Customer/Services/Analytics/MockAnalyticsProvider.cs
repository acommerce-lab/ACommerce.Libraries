using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace ACommerce.Templates.Customer.Services.Analytics;

/// <summary>
/// Mock Analytics Provider for Testing
/// </summary>
public class MockAnalyticsProvider : IAnalyticsProvider
{
    public string ProviderName => "Mock";
    public bool IsInitialized { get; private set; }

    private readonly IJSRuntime? _js;
    private readonly bool _useConsole;

    public MockAnalyticsProvider(IJSRuntime? js = null)
    {
        _js = js;
        _useConsole = js != null;
    }

    public Task InitializeAsync(AnalyticsConfig config)
    {
        IsInitialized = true;
        LogEvent("🚀 Analytics Initialized", new { config.AppId, config.DebugMode });
        return Task.CompletedTask;
    }

    public Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null)
    {
        LogEvent($"📊 Event: {eventName}", parameters);
        return Task.CompletedTask;
    }

    public Task TrackPurchaseAsync(PurchaseEvent purchase)
    {
        LogEvent("💰 Purchase", new
        {
            purchase.TransactionId,
            purchase.Value,
            purchase.Currency,
            Items = purchase.Items.Count
        });
        return Task.CompletedTask;
    }

    public Task TrackScreenViewAsync(string screenName, Dictionary<string, object>? parameters = null)
    {
        LogEvent($"📱 Screen: {screenName}", parameters);
        return Task.CompletedTask;
    }

    public Task TrackRegistrationAsync(string method, Dictionary<string, object>? parameters = null)
    {
        LogEvent($"👤 Registration: {method}", parameters);
        return Task.CompletedTask;
    }

    public Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null)
    {
        LogEvent($"🔑 Login: {method}", parameters);
        return Task.CompletedTask;
    }

    public Task TrackContentViewAsync(ContentViewEvent content)
    {
        LogEvent($"👁️ Content View: {content.ContentName}", new
        {
            content.ContentId,
            content.ContentType,
            content.Category,
            content.Value
        });
        return Task.CompletedTask;
    }

    public Task TrackSearchAsync(string searchTerm, Dictionary<string, object>? parameters = null)
    {
        LogEvent($"🔍 Search: {searchTerm}", parameters);
        return Task.CompletedTask;
    }

    public Task TrackAddToWishlistAsync(ContentViewEvent content)
    {
        LogEvent($"❤️ Add to Wishlist: {content.ContentName}", new { content.ContentId });
        return Task.CompletedTask;
    }

    public Task TrackShareAsync(string contentType, string contentId, string method)
    {
        LogEvent($"📤 Share: {contentType}", new { contentId, method });
        return Task.CompletedTask;
    }

    public Task SetUserIdAsync(string userId)
    {
        LogEvent($"🆔 Set User ID: {userId}", null);
        return Task.CompletedTask;
    }

    public Task SetUserPropertiesAsync(Dictionary<string, object> properties)
    {
        LogEvent("📋 User Properties", properties);
        return Task.CompletedTask;
    }

    private void LogEvent(string message, object? data)
    {
        var logMessage = data != null
            ? $"[MockAnalytics] {message} | {System.Text.Json.JsonSerializer.Serialize(data)}"
            : $"[MockAnalytics] {message}";

        Console.WriteLine(logMessage);

        if (_useConsole && _js != null)
        {
            try
            {
                _ = _js.InvokeVoidAsync("console.log", logMessage);
            }
            catch { }
        }
    }
}

/// <summary>
/// Extension to add Mock provider for testing
/// </summary>
public static class MockAnalyticsExtensions
{
    public static IServiceCollection AddMockAnalyticsProvider(this IServiceCollection services)
    {
        services.AddScoped<MockAnalyticsProvider>();
        services.AddScoped<AnalyticsService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AnalyticsOptions>>();
            var service = new AnalyticsService(options);
            service.AddProvider(sp.GetRequiredService<MockAnalyticsProvider>());
            return service;
        });

        return services;
    }
}
