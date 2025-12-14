namespace Ashare.Shared.Services.Analytics;

public interface IAnalyticsProvider
{
    string ProviderName { get; }
    Task InitializeAsync();
    Task TrackScreenViewAsync(string screenName, Dictionary<string, object>? parameters = null);
    Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null);
    Task TrackContentViewAsync(string contentId, string contentType, string contentName, Dictionary<string, object>? parameters = null);
    Task TrackSearchAsync(string searchQuery, Dictionary<string, object>? parameters = null);
    Task TrackPurchaseAsync(string transactionId, decimal value, string currency = "SAR", Dictionary<string, object>? parameters = null);
    Task TrackRegistrationAsync(string method, Dictionary<string, object>? parameters = null);
    Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null);
    Task TrackAddToWishlistAsync(string contentId, string contentType, Dictionary<string, object>? parameters = null);
    Task TrackShareAsync(string contentId, string contentType, string method, Dictionary<string, object>? parameters = null);
    Task SetUserIdAsync(string userId);
    Task SetUserPropertyAsync(string name, string value);
}
