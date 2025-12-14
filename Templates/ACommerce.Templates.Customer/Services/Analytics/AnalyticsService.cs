using Microsoft.Extensions.Options;

namespace ACommerce.Templates.Customer.Services.Analytics;

/// <summary>
/// Unified Analytics Service that aggregates all analytics providers
/// Sends events to all configured providers simultaneously
/// </summary>
public class AnalyticsService
{
    private readonly List<IAnalyticsProvider> _providers = new();
    private readonly AnalyticsOptions _options;
    private bool _isInitialized;

    public AnalyticsService(IOptions<AnalyticsOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Add an analytics provider
    /// </summary>
    public void AddProvider(IAnalyticsProvider provider)
    {
        _providers.Add(provider);
    }

    /// <summary>
    /// Initialize all providers with their configurations
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        var tasks = new List<Task>();

        foreach (var provider in _providers)
        {
            var config = provider.ProviderName switch
            {
                "Meta" => _options.Meta,
                "Google" => _options.Google,
                "TikTok" => _options.TikTok,
                "Snapchat" => _options.Snapchat,
                _ => null
            };

            if (config != null && !string.IsNullOrEmpty(config.AppId))
            {
                tasks.Add(provider.InitializeAsync(config));
            }
        }

        await Task.WhenAll(tasks);
        _isInitialized = true;
    }

    /// <summary>
    /// Track a custom event across all providers
    /// </summary>
    public async Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null)
    {
        await EnsureInitializedAsync();
        await ExecuteOnAllProvidersAsync(p => p.TrackEventAsync(eventName, parameters));
    }

    /// <summary>
    /// Track a purchase event across all providers
    /// </summary>
    public async Task TrackPurchaseAsync(PurchaseEvent purchase)
    {
        await EnsureInitializedAsync();
        await ExecuteOnAllProvidersAsync(p => p.TrackPurchaseAsync(purchase));
    }

    /// <summary>
    /// Track screen/page view across all providers
    /// </summary>
    public async Task TrackScreenViewAsync(string screenName, Dictionary<string, object>? parameters = null)
    {
        await EnsureInitializedAsync();
        await ExecuteOnAllProvidersAsync(p => p.TrackScreenViewAsync(screenName, parameters));
    }

    /// <summary>
    /// Track user registration across all providers
    /// </summary>
    public async Task TrackRegistrationAsync(string method = "email", Dictionary<string, object>? parameters = null)
    {
        await EnsureInitializedAsync();
        await ExecuteOnAllProvidersAsync(p => p.TrackRegistrationAsync(method, parameters));
    }

    /// <summary>
    /// Track user login across all providers
    /// </summary>
    public async Task TrackLoginAsync(string method = "email", Dictionary<string, object>? parameters = null)
    {
        await EnsureInitializedAsync();
        await ExecuteOnAllProvidersAsync(p => p.TrackLoginAsync(method, parameters));
    }

    /// <summary>
    /// Track content view (listing, product, etc.) across all providers
    /// </summary>
    public async Task TrackContentViewAsync(ContentViewEvent content)
    {
        await EnsureInitializedAsync();
        await ExecuteOnAllProvidersAsync(p => p.TrackContentViewAsync(content));
    }

    /// <summary>
    /// Track search event across all providers
    /// </summary>
    public async Task TrackSearchAsync(string searchTerm, Dictionary<string, object>? parameters = null)
    {
        await EnsureInitializedAsync();
        await ExecuteOnAllProvidersAsync(p => p.TrackSearchAsync(searchTerm, parameters));
    }

    /// <summary>
    /// Track add to wishlist/favorites across all providers
    /// </summary>
    public async Task TrackAddToWishlistAsync(ContentViewEvent content)
    {
        await EnsureInitializedAsync();
        await ExecuteOnAllProvidersAsync(p => p.TrackAddToWishlistAsync(content));
    }

    /// <summary>
    /// Track share event across all providers
    /// </summary>
    public async Task TrackShareAsync(string contentType, string contentId, string method)
    {
        await EnsureInitializedAsync();
        await ExecuteOnAllProvidersAsync(p => p.TrackShareAsync(contentType, contentId, method));
    }

    /// <summary>
    /// Set user ID for tracking across all providers
    /// </summary>
    public async Task SetUserIdAsync(string userId)
    {
        await EnsureInitializedAsync();
        await ExecuteOnAllProvidersAsync(p => p.SetUserIdAsync(userId));
    }

    /// <summary>
    /// Set user properties across all providers
    /// </summary>
    public async Task SetUserPropertiesAsync(Dictionary<string, object> properties)
    {
        await EnsureInitializedAsync();
        await ExecuteOnAllProvidersAsync(p => p.SetUserPropertiesAsync(properties));
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_isInitialized)
        {
            await InitializeAsync();
        }
    }

    private async Task ExecuteOnAllProvidersAsync(Func<IAnalyticsProvider, Task> action)
    {
        var tasks = _providers
            .Where(p => p.IsInitialized)
            .Select(p => SafeExecuteAsync(p, action));

        await Task.WhenAll(tasks);
    }

    private async Task SafeExecuteAsync(IAnalyticsProvider provider, Func<IAnalyticsProvider, Task> action)
    {
        try
        {
            await action(provider);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Analytics] Error in {provider.ProviderName}: {ex.Message}");
        }
    }
}

/// <summary>
/// Analytics configuration options
/// </summary>
public class AnalyticsOptions
{
    public const string SectionName = "Analytics";

    /// <summary>
    /// Enable/disable all analytics
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Meta (Facebook/Instagram) configuration
    /// </summary>
    public AnalyticsConfig? Meta { get; set; }

    /// <summary>
    /// Google Analytics / Firebase configuration
    /// </summary>
    public AnalyticsConfig? Google { get; set; }

    /// <summary>
    /// TikTok configuration
    /// </summary>
    public AnalyticsConfig? TikTok { get; set; }

    /// <summary>
    /// Snapchat configuration
    /// </summary>
    public AnalyticsConfig? Snapchat { get; set; }
}
