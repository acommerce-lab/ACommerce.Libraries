using Microsoft.JSInterop;

namespace ACommerce.Templates.Customer.Services.Analytics.Providers;

/// <summary>
/// Meta (Facebook/Instagram) Analytics Provider
/// Uses Facebook Pixel for Web tracking
/// </summary>
public class MetaAnalyticsProvider : IAnalyticsProvider
{
    public string ProviderName => "Meta";
    public bool IsInitialized { get; private set; }

    private readonly IJSRuntime _js;
    private string _pixelId = "";

    public MetaAnalyticsProvider(IJSRuntime js)
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
            await _js.InvokeVoidAsync("fbq", "init", _pixelId);
            await _js.InvokeVoidAsync("fbq", "track", "PageView");
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Meta Analytics] Init failed: {ex.Message}");
        }
    }

    public async Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        try
        {
            if (parameters != null)
                await _js.InvokeVoidAsync("fbq", "track", eventName, parameters);
            else
                await _js.InvokeVoidAsync("fbq", "track", eventName);
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
                ["currency"] = purchase.Currency,
                ["content_type"] = purchase.ContentType ?? "product",
                ["order_id"] = purchase.TransactionId
            };

            if (purchase.Items.Any())
            {
                parameters["contents"] = purchase.Items.Select(i => new
                {
                    id = i.ItemId,
                    quantity = i.Quantity,
                    item_price = i.Price
                }).ToList();
                parameters["content_ids"] = purchase.Items.Select(i => i.ItemId).ToList();
                parameters["num_items"] = purchase.Items.Sum(i => i.Quantity);
            }

            await _js.InvokeVoidAsync("fbq", "track", "Purchase", parameters);
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

            await _js.InvokeVoidAsync("fbq", "track", "ViewContent", data);
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
                ["registration_method"] = method
            };

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    data[kvp.Key] = kvp.Value;
            }

            await _js.InvokeVoidAsync("fbq", "track", "CompleteRegistration", data);
        }
        catch { }
    }

    public async Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        try
        {
            await _js.InvokeVoidAsync("fbq", "trackCustom", "Login", new { method });
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
                ["content_ids"] = new[] { content.ContentId },
                ["content_name"] = content.ContentName,
                ["content_type"] = content.ContentType,
                ["currency"] = content.Currency
            };

            if (!string.IsNullOrEmpty(content.Category))
                parameters["content_category"] = content.Category;

            if (content.Value.HasValue)
                parameters["value"] = content.Value.Value;

            await _js.InvokeVoidAsync("fbq", "track", "ViewContent", parameters);
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
                ["search_string"] = searchTerm
            };

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    data[kvp.Key] = kvp.Value;
            }

            await _js.InvokeVoidAsync("fbq", "track", "Search", data);
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
                ["content_ids"] = new[] { content.ContentId },
                ["content_name"] = content.ContentName,
                ["content_type"] = content.ContentType,
                ["currency"] = content.Currency
            };

            if (content.Value.HasValue)
                parameters["value"] = content.Value.Value;

            await _js.InvokeVoidAsync("fbq", "track", "AddToWishlist", parameters);
        }
        catch { }
    }

    public async Task TrackShareAsync(string contentType, string contentId, string method)
    {
        if (!IsInitialized) return;

        try
        {
            await _js.InvokeVoidAsync("fbq", "trackCustom", "Share", new
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
        if (!IsInitialized || string.IsNullOrEmpty(_pixelId)) return;

        try
        {
            await _js.InvokeVoidAsync("fbq", "init", _pixelId, new { external_id = userId });
        }
        catch { }
    }

    public Task SetUserPropertiesAsync(Dictionary<string, object> properties)
    {
        return Task.CompletedTask;
    }
}
