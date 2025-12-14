using Microsoft.Extensions.Options;

namespace Ashare.Shared.Services.Analytics;

public class AnalyticsService
{
    private readonly AnalyticsOptions _options;
    private readonly List<IAnalyticsProvider> _providers = new();
    private bool _initialized;

    public AnalyticsService(IOptions<AnalyticsOptions> options)
    {
        _options = options.Value;
    }

    public void AddProvider(IAnalyticsProvider provider)
    {
        _providers.Add(provider);
    }

    public async Task InitializeAsync()
    {
        if (_initialized || !_options.Enabled) return;

        foreach (var provider in _providers)
        {
            try
            {
                await provider.InitializeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Analytics] Failed to initialize {provider.ProviderName}: {ex.Message}");
            }
        }

        _initialized = true;
    }

    public async Task TrackScreenViewAsync(string screenName, Dictionary<string, object>? parameters = null)
    {
        if (!_options.Enabled) return;
        await ExecuteOnAllProviders(p => p.TrackScreenViewAsync(screenName, parameters));
    }

    public async Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null)
    {
        if (!_options.Enabled) return;
        await ExecuteOnAllProviders(p => p.TrackEventAsync(eventName, parameters));
    }

    public async Task TrackContentViewAsync(string contentId, string contentType, string contentName, Dictionary<string, object>? parameters = null)
    {
        if (!_options.Enabled) return;
        await ExecuteOnAllProviders(p => p.TrackContentViewAsync(contentId, contentType, contentName, parameters));
    }

    public async Task TrackSearchAsync(string searchQuery, Dictionary<string, object>? parameters = null)
    {
        if (!_options.Enabled) return;
        await ExecuteOnAllProviders(p => p.TrackSearchAsync(searchQuery, parameters));
    }

    public async Task TrackPurchaseAsync(string transactionId, decimal value, string currency = "SAR", Dictionary<string, object>? parameters = null)
    {
        if (!_options.Enabled) return;
        await ExecuteOnAllProviders(p => p.TrackPurchaseAsync(transactionId, value, currency, parameters));
    }

    public async Task TrackRegistrationAsync(string method, Dictionary<string, object>? parameters = null)
    {
        if (!_options.Enabled) return;
        await ExecuteOnAllProviders(p => p.TrackRegistrationAsync(method, parameters));
    }

    public async Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null)
    {
        if (!_options.Enabled) return;
        await ExecuteOnAllProviders(p => p.TrackLoginAsync(method, parameters));
    }

    public async Task TrackAddToWishlistAsync(string contentId, string contentType, Dictionary<string, object>? parameters = null)
    {
        if (!_options.Enabled) return;
        await ExecuteOnAllProviders(p => p.TrackAddToWishlistAsync(contentId, contentType, parameters));
    }

    public async Task TrackShareAsync(string contentId, string contentType, string method, Dictionary<string, object>? parameters = null)
    {
        if (!_options.Enabled) return;
        await ExecuteOnAllProviders(p => p.TrackShareAsync(contentId, contentType, method, parameters));
    }

    public async Task SetUserIdAsync(string userId)
    {
        if (!_options.Enabled) return;
        await ExecuteOnAllProviders(p => p.SetUserIdAsync(userId));
    }

    public async Task SetUserPropertyAsync(string name, string value)
    {
        if (!_options.Enabled) return;
        await ExecuteOnAllProviders(p => p.SetUserPropertyAsync(name, value));
    }

    private async Task ExecuteOnAllProviders(Func<IAnalyticsProvider, Task> action)
    {
        foreach (var provider in _providers)
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
}
