using Microsoft.JSInterop;

namespace Ashare.Shared.Services.Analytics.Providers;

/// <summary>
/// Snapchat Analytics Provider
/// Uses Snap Pixel for Web tracking
///
/// Setup for Web:
/// 1. Go to https://ads.snapchat.com
/// 2. Events Manager → Create Pixel
/// 3. Get Pixel ID
///
/// Setup for Mobile:
/// 1. Go to https://ads.snapchat.com
/// 2. Events Manager → Mobile App
/// 3. Add App with iOS Bundle ID / Android Package Name
/// 4. Get Snap App ID
/// </summary>
public class SnapchatAnalyticsProvider : IAnalyticsProvider
{
    public string ProviderName => "Snapchat";
    public bool IsInitialized { get; private set; }

    private readonly IJSRuntime _js;
    private string _pixelId = "";

    public SnapchatAnalyticsProvider(IJSRuntime js)
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
            await _js.InvokeVoidAsync("snaptr", "init", _pixelId);
            await _js.InvokeVoidAsync("snaptr", "track", "PAGE_VIEW");
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Snapchat Analytics] Init failed: {ex.Message}");
        }
    }

    public async Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        try
        {
            if (parameters != null)
                await _js.InvokeVoidAsync("snaptr", "track", eventName, parameters);
            else
                await _js.InvokeVoidAsync("snaptr", "track", eventName);
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
                ["price"] = purchase.Value,
                ["currency"] = purchase.Currency,
                ["transaction_id"] = purchase.TransactionId
            };

            if (purchase.Items.Any())
            {
                parameters["item_ids"] = purchase.Items.Select(i => i.ItemId).ToList();
                parameters["number_items"] = purchase.Items.Sum(i => i.Quantity);
            }

            await _js.InvokeVoidAsync("snaptr", "track", "PURCHASE", parameters);
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

            await _js.InvokeVoidAsync("snaptr", "track", "VIEW_CONTENT", data);
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
                ["sign_up_method"] = method
            };

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    data[kvp.Key] = kvp.Value;
            }

            await _js.InvokeVoidAsync("snaptr", "track", "SIGN_UP", data);
        }
        catch { }
    }

    public async Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        try
        {
            await _js.InvokeVoidAsync("snaptr", "track", "LOGIN", new { method });
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
                ["item_ids"] = new[] { content.ContentId },
                ["content_name"] = content.ContentName,
                ["content_type"] = content.ContentType
            };

            if (!string.IsNullOrEmpty(content.Category))
                parameters["content_category"] = content.Category;

            if (content.Value.HasValue)
            {
                parameters["price"] = content.Value.Value;
                parameters["currency"] = content.Currency;
            }

            await _js.InvokeVoidAsync("snaptr", "track", "VIEW_CONTENT", parameters);
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

            await _js.InvokeVoidAsync("snaptr", "track", "SEARCH", data);
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
                ["item_ids"] = new[] { content.ContentId },
                ["content_name"] = content.ContentName
            };

            if (content.Value.HasValue)
            {
                parameters["price"] = content.Value.Value;
                parameters["currency"] = content.Currency;
            }

            await _js.InvokeVoidAsync("snaptr", "track", "ADD_TO_WISHLIST", parameters);
        }
        catch { }
    }

    public async Task TrackShareAsync(string contentType, string contentId, string method)
    {
        if (!IsInitialized) return;

        try
        {
            await _js.InvokeVoidAsync("snaptr", "track", "SHARE", new
            {
                content_type = contentType,
                item_id = contentId,
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
            await _js.InvokeVoidAsync("snaptr", "setUserData", new { user_id = userId });
        }
        catch { }
    }

    public Task SetUserPropertiesAsync(Dictionary<string, object> properties)
    {
        // Snapchat Pixel doesn't support custom user properties
        return Task.CompletedTask;
    }
}
