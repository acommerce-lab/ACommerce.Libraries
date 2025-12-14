using ACommerce.Templates.Customer.Services.Analytics;

namespace Ashare.Shared.Services;

/// <summary>
/// Analytics provider that stores events locally for dashboard display
/// Works alongside other providers (Meta, Google, etc.)
/// </summary>
public class LocalAnalyticsProvider : IAnalyticsProvider
{
    private readonly AnalyticsStore _store;
    private string? _currentUserId;

    public string ProviderName => "Local";
    public bool IsInitialized { get; private set; }

    public LocalAnalyticsProvider(AnalyticsStore store)
    {
        _store = store;
    }

    public Task InitializeAsync(AnalyticsConfig config)
    {
        IsInitialized = true;
        Console.WriteLine("[LocalAnalytics] Initialized - Events will be stored locally for dashboard");
        return Task.CompletedTask;
    }

    public Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null)
    {
        _store.AddEvent(new AnalyticsEvent
        {
            EventType = "custom",
            EventName = eventName,
            UserId = _currentUserId,
            Parameters = parameters
        });
        return Task.CompletedTask;
    }

    public Task TrackPurchaseAsync(PurchaseEvent purchase)
    {
        _store.AddEvent(new AnalyticsEvent
        {
            EventType = "purchase",
            EventName = "purchase",
            UserId = _currentUserId,
            Parameters = new Dictionary<string, object>
            {
                ["transaction_id"] = purchase.TransactionId,
                ["value"] = purchase.Value,
                ["currency"] = purchase.Currency,
                ["content_type"] = purchase.ContentType ?? "unknown",
                ["payment_method"] = purchase.PaymentMethod ?? "unknown",
                ["items_count"] = purchase.Items.Count
            }
        });
        return Task.CompletedTask;
    }

    public Task TrackScreenViewAsync(string screenName, Dictionary<string, object>? parameters = null)
    {
        var eventParams = new Dictionary<string, object>
        {
            ["screen_name"] = screenName
        };

        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                eventParams[kvp.Key] = kvp.Value;
            }
        }

        _store.AddEvent(new AnalyticsEvent
        {
            EventType = "screen_view",
            EventName = screenName,
            UserId = _currentUserId,
            Parameters = eventParams
        });
        return Task.CompletedTask;
    }

    public Task TrackRegistrationAsync(string method, Dictionary<string, object>? parameters = null)
    {
        var eventParams = new Dictionary<string, object>
        {
            ["method"] = method
        };

        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                eventParams[kvp.Key] = kvp.Value;
            }
        }

        _store.AddEvent(new AnalyticsEvent
        {
            EventType = "registration",
            EventName = "sign_up",
            UserId = _currentUserId,
            Parameters = eventParams
        });
        return Task.CompletedTask;
    }

    public Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null)
    {
        var eventParams = new Dictionary<string, object>
        {
            ["method"] = method
        };

        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                eventParams[kvp.Key] = kvp.Value;
            }
        }

        _store.AddEvent(new AnalyticsEvent
        {
            EventType = "login",
            EventName = "login",
            UserId = _currentUserId,
            Parameters = eventParams
        });
        return Task.CompletedTask;
    }

    public Task TrackContentViewAsync(ContentViewEvent content)
    {
        _store.AddEvent(new AnalyticsEvent
        {
            EventType = "content_view",
            EventName = "view_item",
            UserId = _currentUserId,
            Parameters = new Dictionary<string, object>
            {
                ["content_id"] = content.ContentId,
                ["content_name"] = content.ContentName,
                ["content_type"] = content.ContentType,
                ["category"] = content.Category ?? "unknown",
                ["value"] = content.Value ?? 0,
                ["currency"] = content.Currency
            }
        });
        return Task.CompletedTask;
    }

    public Task TrackSearchAsync(string searchTerm, Dictionary<string, object>? parameters = null)
    {
        var eventParams = new Dictionary<string, object>
        {
            ["search_term"] = searchTerm
        };

        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                eventParams[kvp.Key] = kvp.Value;
            }
        }

        _store.AddEvent(new AnalyticsEvent
        {
            EventType = "search",
            EventName = "search",
            UserId = _currentUserId,
            Parameters = eventParams
        });
        return Task.CompletedTask;
    }

    public Task TrackAddToWishlistAsync(ContentViewEvent content)
    {
        _store.AddEvent(new AnalyticsEvent
        {
            EventType = "add_to_wishlist",
            EventName = "add_to_wishlist",
            UserId = _currentUserId,
            Parameters = new Dictionary<string, object>
            {
                ["content_id"] = content.ContentId,
                ["content_name"] = content.ContentName,
                ["content_type"] = content.ContentType,
                ["value"] = content.Value ?? 0
            }
        });
        return Task.CompletedTask;
    }

    public Task TrackShareAsync(string contentType, string contentId, string method)
    {
        _store.AddEvent(new AnalyticsEvent
        {
            EventType = "share",
            EventName = "share",
            UserId = _currentUserId,
            Parameters = new Dictionary<string, object>
            {
                ["content_type"] = contentType,
                ["content_id"] = contentId,
                ["method"] = method
            }
        });
        return Task.CompletedTask;
    }

    public Task SetUserIdAsync(string userId)
    {
        _currentUserId = userId;
        _store.TrackUser(userId);
        return Task.CompletedTask;
    }

    public Task SetUserPropertiesAsync(Dictionary<string, object> properties)
    {
        _store.AddEvent(new AnalyticsEvent
        {
            EventType = "user_properties",
            EventName = "set_user_properties",
            UserId = _currentUserId,
            Parameters = properties
        });
        return Task.CompletedTask;
    }
}
