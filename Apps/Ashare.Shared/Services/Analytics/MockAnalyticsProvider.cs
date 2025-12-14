namespace Ashare.Shared.Services.Analytics;

public class MockAnalyticsProvider : IAnalyticsProvider
{
    public string ProviderName => "Mock";

    public Task InitializeAsync()
    {
        Console.WriteLine($"[MockAnalytics] 🚀 Initialized");
        return Task.CompletedTask;
    }

    public Task TrackScreenViewAsync(string screenName, Dictionary<string, object>? parameters = null)
    {
        Console.WriteLine($"[MockAnalytics] 📱 Screen: {screenName} | {FormatParams(parameters)}");
        return Task.CompletedTask;
    }

    public Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null)
    {
        Console.WriteLine($"[MockAnalytics] ⚡ Event: {eventName} | {FormatParams(parameters)}");
        return Task.CompletedTask;
    }

    public Task TrackContentViewAsync(string contentId, string contentType, string contentName, Dictionary<string, object>? parameters = null)
    {
        Console.WriteLine($"[MockAnalytics] 👁️ Content View: {contentName} | {FormatParams(new Dictionary<string, object> { ["ContentId"] = contentId, ["ContentType"] = contentType })}");
        return Task.CompletedTask;
    }

    public Task TrackSearchAsync(string searchQuery, Dictionary<string, object>? parameters = null)
    {
        Console.WriteLine($"[MockAnalytics] 🔍 Search: {searchQuery}");
        return Task.CompletedTask;
    }

    public Task TrackPurchaseAsync(string transactionId, decimal value, string currency = "SAR", Dictionary<string, object>? parameters = null)
    {
        Console.WriteLine($"[MockAnalytics] 💰 Purchase | {FormatParams(new Dictionary<string, object> { ["TransactionId"] = transactionId, ["Value"] = value, ["Currency"] = currency })}");
        return Task.CompletedTask;
    }

    public Task TrackRegistrationAsync(string method, Dictionary<string, object>? parameters = null)
    {
        Console.WriteLine($"[MockAnalytics] 📝 Registration: {method}");
        return Task.CompletedTask;
    }

    public Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null)
    {
        Console.WriteLine($"[MockAnalytics] 🔑 Login: {method}");
        return Task.CompletedTask;
    }

    public Task TrackAddToWishlistAsync(string contentId, string contentType, Dictionary<string, object>? parameters = null)
    {
        Console.WriteLine($"[MockAnalytics] ❤️ Add to Wishlist: {contentId} ({contentType})");
        return Task.CompletedTask;
    }

    public Task TrackShareAsync(string contentId, string contentType, string method, Dictionary<string, object>? parameters = null)
    {
        Console.WriteLine($"[MockAnalytics] 📤 Share: {contentId} via {method}");
        return Task.CompletedTask;
    }

    public Task SetUserIdAsync(string userId)
    {
        Console.WriteLine($"[MockAnalytics] 🆔 Set User ID: {userId}");
        return Task.CompletedTask;
    }

    public Task SetUserPropertyAsync(string name, string value)
    {
        Console.WriteLine($"[MockAnalytics] 📊 User Property: {name} = {value}");
        return Task.CompletedTask;
    }

    private static string FormatParams(Dictionary<string, object>? parameters)
    {
        if (parameters == null || parameters.Count == 0) return "";
        return "{" + string.Join(", ", parameters.Select(p => $"\"{p.Key}\":\"{p.Value}\"")) + "}";
    }
}
