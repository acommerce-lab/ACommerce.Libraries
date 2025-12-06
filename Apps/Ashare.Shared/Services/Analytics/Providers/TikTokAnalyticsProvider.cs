using Microsoft.JSInterop;

namespace Ashare.Shared.Services.Analytics.Providers;

/// <summary>
/// TikTok Analytics Provider
/// Uses TikTok Pixel for Web tracking
///
/// Setup for Web:
/// 1. Go to https://ads.tiktok.com
/// 2. Assets → Events → Web Events
/// 3. Create Pixel and get Pixel ID
///
/// Setup for Mobile (App Events):
/// 1. Go to https://ads.tiktok.com
/// 2. Assets → Events → App Events
/// 3. Create App / Add App
/// 4. Add iOS Bundle ID and Android Package Name
/// 5. Get iOS TikTok App ID and Android TikTok App ID
/// </summary>
public class TikTokAnalyticsProvider : IAnalyticsProvider
{
    public string ProviderName => "TikTok";
    public bool IsInitialized { get; private set; }

    private readonly IJSRuntime _js;
    private string _pixelId = "";

    public TikTokAnalyticsProvider(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync(AnalyticsConfig config)
    {
        if (string.IsNullOrEmpty(config.AppId))
            return;

        _pixelId = config.AppId;

        try
        {
            await _js.InvokeVoidAsync("ttq.load", _pixelId);
            await _js.InvokeVoidAsync("ttq.page");
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TikTok Analytics] Init failed: {ex.Message}");
        }
    }

    public async Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        try
        {
            if (parameters != null)
                await _js.InvokeVoidAsync("ttq.track", eventName, parameters);
            else
                await _js.InvokeVoidAsync("ttq.track", eventName);
        }
        catch { }
    }

    public async Task TrackPurchaseAsync(PurchaseEvent purchase)
    {
        if (!IsInitialized) return;

        try
        {
            var parameters = new Dictionary<string, object>
            {
                ["value"] = purchase.Value,
                ["currency"] = purchase.Currency
            };

            if (purchase.Items.Any())
            {
                parameters["contents"] = purchase.Items.Select(i => new
                {
                    content_id = i.ItemId,
                    content_name = i.ItemName,
                    content_category = i.Category ?? "",
                    price = i.Price,
                    quantity = i.Quantity
                }).ToList();
                parameters["content_type"] = purchase.ContentType ?? "product";
            }

            // TikTok uses "CompletePayment" for purchases
            await _js.InvokeVoidAsync("ttq.track", "CompletePayment", parameters);
        }
        catch { }
    }

    public async Task TrackScreenViewAsync(string screenName, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        try
        {
            var data = new Dictionary<string, object>
            {
                ["content_name"] = screenName
            };

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    data[kvp.Key] = kvp.Value;
            }

            await _js.InvokeVoidAsync("ttq.track", "ViewContent", data);
        }
        catch { }
    }

    public async Task TrackRegistrationAsync(string method, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        try
        {
            var data = new Dictionary<string, object>
            {
                ["method"] = method
            };

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    data[kvp.Key] = kvp.Value;
            }

            await _js.InvokeVoidAsync("ttq.track", "CompleteRegistration", data);
        }
        catch { }
    }

    public async Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        try
        {
            // TikTok doesn't have a standard login event, use custom
            await _js.InvokeVoidAsync("ttq.track", "Login", new { method });
        }
        catch { }
    }

    public async Task TrackContentViewAsync(ContentViewEvent content)
    {
        if (!IsInitialized) return;

        try
        {
            var parameters = new Dictionary<string, object>
            {
                ["content_id"] = content.ContentId,
                ["content_name"] = content.ContentName,
                ["content_type"] = content.ContentType,
                ["currency"] = content.Currency
            };

            if (!string.IsNullOrEmpty(content.Category))
                parameters["content_category"] = content.Category;

            if (content.Value.HasValue)
                parameters["value"] = content.Value.Value;

            await _js.InvokeVoidAsync("ttq.track", "ViewContent", parameters);
        }
        catch { }
    }

    public async Task TrackSearchAsync(string searchTerm, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        try
        {
            var data = new Dictionary<string, object>
            {
                ["query"] = searchTerm
            };

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    data[kvp.Key] = kvp.Value;
            }

            await _js.InvokeVoidAsync("ttq.track", "Search", data);
        }
        catch { }
    }

    public async Task TrackAddToWishlistAsync(ContentViewEvent content)
    {
        if (!IsInitialized) return;

        try
        {
            var parameters = new Dictionary<string, object>
            {
                ["content_id"] = content.ContentId,
                ["content_name"] = content.ContentName,
                ["content_type"] = content.ContentType,
                ["currency"] = content.Currency
            };

            if (content.Value.HasValue)
                parameters["value"] = content.Value.Value;

            await _js.InvokeVoidAsync("ttq.track", "AddToWishlist", parameters);
        }
        catch { }
    }

    public async Task TrackShareAsync(string contentType, string contentId, string method)
    {
        if (!IsInitialized) return;

        try
        {
            // TikTok doesn't have a standard share event
            await _js.InvokeVoidAsync("ttq.track", "Share", new
            {
                content_type = contentType,
                content_id = contentId,
                method
            });
        }
        catch { }
    }

    public async Task SetUserIdAsync(string userId)
    {
        if (!IsInitialized) return;

        try
        {
            await _js.InvokeVoidAsync("ttq.identify", new { external_id = userId });
        }
        catch { }
    }

    public Task SetUserPropertiesAsync(Dictionary<string, object> properties)
    {
        // TikTok Pixel doesn't support custom user properties
        return Task.CompletedTask;
    }
}
