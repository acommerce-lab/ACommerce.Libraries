using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ashare.Shared.Services.Analytics.Providers;

public class MetaMobileAnalyticsProvider : IAnalyticsProvider
{
    public string ProviderName => "MetaMobile";
    public bool IsInitialized { get; private set; }

    private readonly HttpClient _httpClient;
    private readonly IAdvertiserIdService? _advertiserIdService;
    private string _appId = "";
    private string _accessToken = "";
    private bool _debugMode;

    private const string FacebookGraphApiUrl = "https://graph.facebook.com/v18.0";

    public MetaMobileAnalyticsProvider(
        HttpClient httpClient,
        IAdvertiserIdService? advertiserIdService = null)
    {
        _httpClient = httpClient;
        _advertiserIdService = advertiserIdService;
    }

    public Task InitializeAsync(AnalyticsConfig config)
    {
        if (string.IsNullOrEmpty(config.AppId))
        {
            Console.WriteLine("[MetaMobile] Missing AppId");
            return Task.CompletedTask;
        }

        _appId = config.AppId;
        _accessToken = config.AccessToken ?? "";
        _debugMode = config.DebugMode;
        IsInitialized = true;

        Console.WriteLine($"[MetaMobile] Initialized with AppId: {_appId}");
        return Task.CompletedTask;
    }

    public async Task TrackEventAsync(string eventName, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        await SendEventAsync(eventName, parameters);
    }

    public async Task TrackPurchaseAsync(PurchaseEvent purchase)
    {
        if (!IsInitialized) return;

        var parameters = new Dictionary<string, object>
        {
            ["_valueToSum"] = purchase.Value,
            ["fb_currency"] = purchase.Currency,
            ["fb_content_type"] = purchase.ContentType ?? "product",
            ["fb_order_id"] = purchase.TransactionId
        };

        if (purchase.Items.Any())
        {
            parameters["fb_content_id"] = string.Join(",", purchase.Items.Select(i => i.ItemId));
            parameters["fb_num_items"] = purchase.Items.Sum(i => i.Quantity);
        }

        await SendEventAsync("fb_mobile_purchase", parameters);
    }

    public async Task TrackScreenViewAsync(string screenName, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        var data = new Dictionary<string, object>
        {
            ["fb_content_name"] = screenName
        };

        if (parameters != null)
        {
            foreach (var kvp in parameters)
                data[kvp.Key] = kvp.Value;
        }

        await SendEventAsync("fb_mobile_content_view", data);
    }

    public async Task TrackRegistrationAsync(string method, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        var data = new Dictionary<string, object>
        {
            ["fb_registration_method"] = method
        };

        if (parameters != null)
        {
            foreach (var kvp in parameters)
                data[kvp.Key] = kvp.Value;
        }

        await SendEventAsync("fb_mobile_complete_registration", data);
    }

    public async Task TrackLoginAsync(string method, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        await SendEventAsync("fb_mobile_login", new Dictionary<string, object>
        {
            ["method"] = method
        });
    }

    public async Task TrackContentViewAsync(ContentViewEvent content)
    {
        if (!IsInitialized) return;

        var parameters = new Dictionary<string, object>
        {
            ["fb_content_id"] = content.ContentId,
            ["fb_content_name"] = content.ContentName,
            ["fb_content_type"] = content.ContentType,
            ["fb_currency"] = content.Currency
        };

        if (!string.IsNullOrEmpty(content.Category))
            parameters["fb_content_category"] = content.Category;

        if (content.Value.HasValue)
            parameters["_valueToSum"] = content.Value.Value;

        await SendEventAsync("fb_mobile_content_view", parameters);
    }

    public async Task TrackSearchAsync(string searchTerm, Dictionary<string, object>? parameters = null)
    {
        if (!IsInitialized) return;

        var data = new Dictionary<string, object>
        {
            ["fb_search_string"] = searchTerm
        };

        if (parameters != null)
        {
            foreach (var kvp in parameters)
                data[kvp.Key] = kvp.Value;
        }

        await SendEventAsync("fb_mobile_search", data);
    }

    public async Task TrackAddToWishlistAsync(ContentViewEvent content)
    {
        if (!IsInitialized) return;

        var parameters = new Dictionary<string, object>
        {
            ["fb_content_id"] = content.ContentId,
            ["fb_content_name"] = content.ContentName,
            ["fb_content_type"] = content.ContentType,
            ["fb_currency"] = content.Currency
        };

        if (content.Value.HasValue)
            parameters["_valueToSum"] = content.Value.Value;

        await SendEventAsync("fb_mobile_add_to_wishlist", parameters);
    }

    public async Task TrackShareAsync(string contentType, string contentId, string method)
    {
        if (!IsInitialized) return;

        await SendEventAsync("fb_mobile_share", new Dictionary<string, object>
        {
            ["fb_content_type"] = contentType,
            ["fb_content_id"] = contentId,
            ["method"] = method
        });
    }

    public Task SetUserIdAsync(string userId)
    {
        return Task.CompletedTask;
    }

    public Task SetUserPropertiesAsync(Dictionary<string, object> properties)
    {
        return Task.CompletedTask;
    }

    public async Task TrackAddToCartAsync(ContentViewEvent content)
    {
        if (!IsInitialized) return;

        var parameters = new Dictionary<string, object>
        {
            ["fb_content_id"] = content.ContentId,
            ["fb_content_name"] = content.ContentName,
            ["fb_content_type"] = content.ContentType,
            ["fb_currency"] = content.Currency
        };

        if (content.Value.HasValue)
            parameters["_valueToSum"] = content.Value.Value;

        await SendEventAsync("fb_mobile_add_to_cart", parameters);
    }

    public async Task TrackInitiateCheckoutAsync(decimal value, string currency, int numItems)
    {
        if (!IsInitialized) return;

        await SendEventAsync("fb_mobile_initiated_checkout", new Dictionary<string, object>
        {
            ["_valueToSum"] = value,
            ["fb_currency"] = currency,
            ["fb_num_items"] = numItems
        });
    }

    public async Task TrackContactAsync()
    {
        if (!IsInitialized) return;

        await SendEventAsync("Contact", null);
    }

    public async Task TrackSubscribeAsync()
    {
        if (!IsInitialized) return;

        await SendEventAsync("Subscribe", null);
    }

    public async Task TrackCompleteTutorialAsync(bool success, string? contentId = null)
    {
        if (!IsInitialized) return;

        var parameters = new Dictionary<string, object>
        {
            ["fb_success"] = success ? 1 : 0
        };

        if (!string.IsNullOrEmpty(contentId))
            parameters["fb_content_id"] = contentId;

        await SendEventAsync("fb_mobile_tutorial_completion", parameters);
    }

    public async Task TrackAchieveLevelAsync(string level)
    {
        if (!IsInitialized) return;

        await SendEventAsync("fb_mobile_level_achieved", new Dictionary<string, object>
        {
            ["fb_level"] = level
        });
    }

    public async Task TrackUnlockAchievementAsync(string description)
    {
        if (!IsInitialized) return;

        await SendEventAsync("fb_mobile_achievement_unlocked", new Dictionary<string, object>
        {
            ["fb_description"] = description
        });
    }

    public async Task TrackSpendCreditsAsync(decimal value, string currency, string? contentId = null)
    {
        if (!IsInitialized) return;

        var parameters = new Dictionary<string, object>
        {
            ["_valueToSum"] = value,
            ["fb_currency"] = currency
        };

        if (!string.IsNullOrEmpty(contentId))
            parameters["fb_content_id"] = contentId;

        await SendEventAsync("fb_mobile_spent_credits", parameters);
    }

    public async Task TrackAddPaymentInfoAsync(bool success)
    {
        if (!IsInitialized) return;

        await SendEventAsync("fb_mobile_add_payment_info", new Dictionary<string, object>
        {
            ["fb_success"] = success ? 1 : 0
        });
    }

    public async Task TrackRateAsync(decimal rating, decimal? maxRating = null, string? contentType = null)
    {
        if (!IsInitialized) return;

        var parameters = new Dictionary<string, object>
        {
            ["_valueToSum"] = rating
        };

        if (maxRating.HasValue)
            parameters["fb_max_rating_value"] = maxRating.Value;

        if (!string.IsNullOrEmpty(contentType))
            parameters["fb_content_type"] = contentType;

        await SendEventAsync("fb_mobile_rate", parameters);
    }

    public async Task TrackStartTrialAsync()
    {
        if (!IsInitialized) return;

        await SendEventAsync("StartTrial", null);
    }

    public async Task TrackScheduleAsync()
    {
        if (!IsInitialized) return;

        await SendEventAsync("Schedule", null);
    }

    public async Task TrackSubmitApplicationAsync()
    {
        if (!IsInitialized) return;

        await SendEventAsync("SubmitApplication", null);
    }

    public async Task TrackFindLocationAsync()
    {
        if (!IsInitialized) return;

        await SendEventAsync("FindLocation", null);
    }

    public async Task TrackDonateAsync()
    {
        if (!IsInitialized) return;

        await SendEventAsync("Donate", null);
    }

    public async Task TrackCustomizeProductAsync()
    {
        if (!IsInitialized) return;

        await SendEventAsync("CustomizeProduct", null);
    }

    private async Task SendEventAsync(string eventName, Dictionary<string, object>? parameters)
    {
        try
        {
            var advertiserId = _advertiserIdService != null
                ? await _advertiserIdService.GetAdvertiserIdAsync()
                : null;

            var customEvent = new Dictionary<string, object>
            {
                ["_eventName"] = eventName,
                ["_logTime"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    customEvent[kvp.Key] = kvp.Value;
                }
            }

            var customEventsJson = JsonSerializer.Serialize(new[] { customEvent }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var formData = new Dictionary<string, string>
            {
                ["event"] = "CUSTOM_APP_EVENTS",
                ["custom_events"] = customEventsJson,
                ["application_tracking_enabled"] = "1"
            };

            if (!string.IsNullOrEmpty(advertiserId))
            {
                formData["advertiser_id"] = advertiserId;
            }

            var url = $"{FacebookGraphApiUrl}/{_appId}/activities";

            if (!string.IsNullOrEmpty(_accessToken))
            {
                url += $"?access_token={_accessToken}";
            }

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(url, content);

            if (_debugMode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[MetaMobile] Event '{eventName}' sent. Status: {response.StatusCode}, Response: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MetaMobile] Failed to send event '{eventName}': {ex.Message}");
        }
    }
}

public interface IAdvertiserIdService
{
    Task<string?> GetAdvertiserIdAsync();
}
