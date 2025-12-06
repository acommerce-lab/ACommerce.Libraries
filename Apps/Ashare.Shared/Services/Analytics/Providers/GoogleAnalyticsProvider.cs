using Microsoft.JSInterop;

namespace Ashare.Shared.Services.Analytics.Providers;

/// <summary>
/// Google Analytics 4 (GA4) Provider
/// Uses gtag.js for Web tracking
///
/// Setup for Web (GA4):
/// 1. Go to https://analytics.google.com
/// 2. Create Property → Web
/// 3. Get Measurement ID (G-XXXXXXXXXX)
///
/// Setup for Mobile (Firebase):
/// 1. Go to https://console.firebase.google.com
/// 2. Create/Select Project
/// 3. Add iOS/Android apps
/// 4. Download GoogleService-Info.plist (iOS) or google-services.json (Android)
/// 5. Get App IDs from Project Settings → Your Apps
///
/// Note: GA4 and Firebase Analytics share the same backend!
/// </summary>
public class GoogleAnalyticsProvider : IAnalyticsProvider
{
    public string ProviderName => "Google";
    public bool IsInitialized { get; private set; }

    private readonly IJSRuntime _js;
    private string _measurementId = "";

    public GoogleAnalyticsProvider(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync(AnalyticsConfig config)
    {
        if (string.IsNullOrEmpty(config.AppId))
            return;

        _measurementId = config.AppId;

        try
        {
            var configOptions = new Dictionary<string, object>();

            if (config.DebugMode)
                configOptions["debug_mode"] = true;

            await _js.InvokeVoidAsync("gtag", "config", _measurementId, configOptions);
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Google Analytics] Init failed: {ex.Message}");
        }
    }

    public async Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        try
        {
            if (parameters != null)
                await _js.InvokeVoidAsync("gtag", "event", eventName, parameters);
            else
                await _js.InvokeVoidAsync("gtag", "event", eventName);
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
                ["transaction_id"] = purchase.TransactionId,
                ["value"] = purchase.Value,
                ["currency"] = purchase.Currency
            };

            if (!string.IsNullOrEmpty(purchase.PaymentMethod))
                parameters["payment_type"] = purchase.PaymentMethod;

            if (purchase.Items.Any())
            {
                parameters["items"] = purchase.Items.Select(i => new Dictionary<string, object>
                {
                    ["item_id"] = i.ItemId,
                    ["item_name"] = i.ItemName,
                    ["item_category"] = i.Category ?? "",
                    ["price"] = i.Price,
                    ["quantity"] = i.Quantity
                }).ToList();
            }

            await _js.InvokeVoidAsync("gtag", "event", "purchase", parameters);
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
                ["screen_name"] = screenName
            };

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    data[kvp.Key] = kvp.Value;
            }

            await _js.InvokeVoidAsync("gtag", "event", "screen_view", data);
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

            await _js.InvokeVoidAsync("gtag", "event", "sign_up", data);
        }
        catch { }
    }

    public async Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null)
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

            await _js.InvokeVoidAsync("gtag", "event", "login", data);
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
                ["content_type"] = content.ContentType,
                ["item_id"] = content.ContentId,
                ["items"] = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["item_id"] = content.ContentId,
                        ["item_name"] = content.ContentName,
                        ["item_category"] = content.Category ?? "",
                        ["price"] = content.Value ?? 0
                    }
                }
            };

            if (content.Value.HasValue)
            {
                parameters["value"] = content.Value.Value;
                parameters["currency"] = content.Currency;
            }

            await _js.InvokeVoidAsync("gtag", "event", "view_item", parameters);
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
                ["search_term"] = searchTerm
            };

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                    data[kvp.Key] = kvp.Value;
            }

            await _js.InvokeVoidAsync("gtag", "event", "search", data);
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
                ["currency"] = content.Currency,
                ["items"] = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["item_id"] = content.ContentId,
                        ["item_name"] = content.ContentName,
                        ["item_category"] = content.Category ?? "",
                        ["price"] = content.Value ?? 0
                    }
                }
            };

            if (content.Value.HasValue)
                parameters["value"] = content.Value.Value;

            await _js.InvokeVoidAsync("gtag", "event", "add_to_wishlist", parameters);
        }
        catch { }
    }

    public async Task TrackShareAsync(string contentType, string contentId, string method)
    {
        if (!IsInitialized) return;

        try
        {
            await _js.InvokeVoidAsync("gtag", "event", "share", new
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
            await _js.InvokeVoidAsync("gtag", "set", new { user_id = userId });
        }
        catch { }
    }

    public async Task SetUserPropertiesAsync(Dictionary<string, object> properties)
    {
        if (!IsInitialized) return;

        try
        {
            await _js.InvokeVoidAsync("gtag", "set", "user_properties", properties);
        }
        catch { }
    }
}
