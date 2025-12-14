namespace ACommerce.Templates.Customer.Services.Analytics;

/// <summary>
/// Unified interface for all analytics providers (Meta, Google, TikTok, Snapchat)
/// Works for both Web (Pixels) and Mobile (SDKs)
/// </summary>
public interface IAnalyticsProvider
{
    /// <summary>
    /// Provider name (Meta, Google, TikTok, Snapchat)
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Whether this provider is initialized and ready
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Initialize the analytics provider with configuration
    /// </summary>
    Task InitializeAsync(AnalyticsConfig config);

    /// <summary>
    /// Track a custom event
    /// </summary>
    Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Track a purchase/payment event
    /// </summary>
    Task TrackPurchaseAsync(PurchaseEvent purchase);

    /// <summary>
    /// Track screen/page view
    /// </summary>
    Task TrackScreenViewAsync(string screenName, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Track user registration
    /// </summary>
    Task TrackRegistrationAsync(string method, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Track user login
    /// </summary>
    Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Track content view (listing, product, etc.)
    /// </summary>
    Task TrackContentViewAsync(ContentViewEvent content);

    /// <summary>
    /// Track search event
    /// </summary>
    Task TrackSearchAsync(string searchTerm, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Track add to wishlist/favorites
    /// </summary>
    Task TrackAddToWishlistAsync(ContentViewEvent content);

    /// <summary>
    /// Track share event
    /// </summary>
    Task TrackShareAsync(string contentType, string contentId, string method);

    /// <summary>
    /// Set user ID for tracking
    /// </summary>
    Task SetUserIdAsync(string userId);

    /// <summary>
    /// Set user properties
    /// </summary>
    Task SetUserPropertiesAsync(Dictionary<string, object> properties);
}

/// <summary>
/// Analytics configuration
/// </summary>
public class AnalyticsConfig
{
    /// <summary>
    /// App ID or Pixel ID
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// API Key (if required)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Access Token (for server-side events)
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Client Token (required for Facebook Mobile SDK)
    /// Get from: Facebook Developers Console → Settings → Advanced → Client Token
    /// </summary>
    public string? ClientToken { get; set; }

    /// <summary>
    /// Enable debug mode
    /// </summary>
    public bool DebugMode { get; set; }

    /// <summary>
    /// iOS-specific App ID (for mobile SDKs)
    /// </summary>
    public string? IosAppId { get; set; }

    /// <summary>
    /// Android-specific App ID (for mobile SDKs)
    /// </summary>
    public string? AndroidAppId { get; set; }
}

/// <summary>
/// Purchase event data
/// </summary>
public class PurchaseEvent
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? ContentType { get; set; }
    public string? PaymentMethod { get; set; }
    public List<PurchaseItem> Items { get; set; } = new();
    public Dictionary<string, object>? AdditionalData { get; set; }
}

/// <summary>
/// Purchase item data
/// </summary>
public class PurchaseItem
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Content view event data (for listings, products, etc.)
/// </summary>
public class ContentViewEvent
{
    public string ContentId { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "listing";
    public string? Category { get; set; }
    public decimal? Value { get; set; }
    public string Currency { get; set; } = "SAR";
    public Dictionary<string, object>? AdditionalData { get; set; }
}
