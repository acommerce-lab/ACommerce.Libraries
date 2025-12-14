using Microsoft.Extensions.Options;

namespace Ashare.Shared.Services.Analytics.Providers;

public class TikTokAnalyticsProvider : IAnalyticsProvider
{
    private readonly AnalyticsConfig _config;
    public string ProviderName => "TikTok";

    public TikTokAnalyticsProvider(IOptions<AnalyticsOptions> options)
    {
        _config = options.Value.TikTok;
    }

    public Task InitializeAsync()
    {
        if (_config.DebugMode)
            Console.WriteLine($"[TikTok] Initialized with AppId: {_config.AppId}");
        return Task.CompletedTask;
    }

    public Task TrackScreenViewAsync(string screenName, Dictionary<string, object>? parameters = null) => Task.CompletedTask;
    public Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null) => Task.CompletedTask;
    public Task TrackContentViewAsync(string contentId, string contentType, string contentName, Dictionary<string, object>? parameters = null) => Task.CompletedTask;
    public Task TrackSearchAsync(string searchQuery, Dictionary<string, object>? parameters = null) => Task.CompletedTask;
    public Task TrackPurchaseAsync(string transactionId, decimal value, string currency = "SAR", Dictionary<string, object>? parameters = null) => Task.CompletedTask;
    public Task TrackRegistrationAsync(string method, Dictionary<string, object>? parameters = null) => Task.CompletedTask;
    public Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null) => Task.CompletedTask;
    public Task TrackAddToWishlistAsync(string contentId, string contentType, Dictionary<string, object>? parameters = null) => Task.CompletedTask;
    public Task TrackShareAsync(string contentId, string contentType, string method, Dictionary<string, object>? parameters = null) => Task.CompletedTask;
    public Task SetUserIdAsync(string userId) => Task.CompletedTask;
    public Task SetUserPropertyAsync(string name, string value) => Task.CompletedTask;
}
