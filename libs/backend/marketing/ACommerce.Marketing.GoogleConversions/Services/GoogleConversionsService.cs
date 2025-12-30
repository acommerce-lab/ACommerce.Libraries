using System.Net.Http.Json;
using System.Text.Json;
using ACommerce.Marketing.GoogleConversions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.Marketing.GoogleConversions.Services;

public class GoogleConversionsService : IGoogleConversionsService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleConversionsOptions _options;
    private readonly ILogger<GoogleConversionsService> _logger;

    private const string BaseUrl = "https://www.google-analytics.com/mp/collect";
    private const string DebugUrl = "https://www.google-analytics.com/debug/mp/collect";

    public GoogleConversionsService(
        HttpClient httpClient,
        IOptions<GoogleConversionsOptions> options,
        ILogger<GoogleConversionsService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> TrackPurchaseAsync(GooglePurchaseEventRequest request)
    {
        var eventParams = new Dictionary<string, object>
        {
            ["transaction_id"] = request.TransactionId,
            ["value"] = request.Value,
            ["currency"] = request.Currency
        };

        if (!string.IsNullOrEmpty(request.Coupon))
            eventParams["coupon"] = request.Coupon;

        if (request.Items?.Any() == true)
        {
            eventParams["items"] = request.Items.Select(i => new Dictionary<string, object?>
            {
                ["item_id"] = i.ItemId,
                ["item_name"] = i.ItemName,
                ["item_category"] = i.ItemCategory,
                ["price"] = i.Price,
                ["quantity"] = i.Quantity
            }).ToList();
        }

        return await SendEventAsync("purchase", eventParams, request.User);
    }

    public async Task<bool> TrackViewContentAsync(GoogleViewContentEventRequest request)
    {
        var eventParams = new Dictionary<string, object>
        {
            ["items"] = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["item_id"] = request.ContentId,
                    ["item_name"] = request.ContentName,
                    ["item_category"] = request.Category,
                    ["price"] = request.Value ?? 0
                }
            }
        };

        return await SendEventAsync("view_item", eventParams, request.User);
    }

    public async Task<bool> TrackSearchAsync(GoogleSearchEventRequest request)
    {
        var eventParams = new Dictionary<string, object>
        {
            ["search_term"] = request.SearchQuery
        };

        return await SendEventAsync("search", eventParams, request.User);
    }

    public async Task<bool> TrackRegistrationAsync(GoogleRegistrationEventRequest request)
    {
        var eventParams = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(request.Method))
            eventParams["method"] = request.Method;

        return await SendEventAsync("sign_up", eventParams, request.User);
    }

    public async Task<bool> TrackLoginAsync(GoogleLoginEventRequest request)
    {
        var eventParams = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(request.Method))
            eventParams["method"] = request.Method;

        return await SendEventAsync("login", eventParams, request.User);
    }

    public async Task<bool> TrackAddToWishlistAsync(GoogleAddToWishlistEventRequest request)
    {
        var eventParams = new Dictionary<string, object>
        {
            ["items"] = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["item_id"] = request.ContentId,
                    ["item_name"] = request.ContentName,
                    ["item_category"] = request.ContentType,
                    ["price"] = request.Value ?? 0
                }
            }
        };

        return await SendEventAsync("add_to_wishlist", eventParams, request.User);
    }

    public async Task<bool> TrackCustomEventAsync(GoogleCustomEventRequest request)
    {
        return await SendEventAsync(request.EventName, request.Params ?? new(), request.User);
    }

    private async Task<bool> SendEventAsync(string eventName, Dictionary<string, object> eventParams, GoogleUserContext? user)
    {
        if (!_options.IsEnabled)
        {
            _logger.LogWarning("[GoogleConversions] Service not configured. Skipping event: {EventName}", eventName);
            return false;
        }

        try
        {
            var clientId = user?.ClientId ?? user?.Gclid ?? Guid.NewGuid().ToString();

            var payload = new GA4EventPayload
            {
                ClientId = clientId,
                UserId = user?.UserId,
                TimestampMicros = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000,
                Events = new List<GA4Event>
                {
                    new()
                    {
                        Name = eventName,
                        Params = eventParams
                    }
                }
            };

            // Add session_id if available
            if (!string.IsNullOrEmpty(user?.SessionId))
            {
                payload.Events[0].Params["session_id"] = user.SessionId;
            }

            // Add engagement_time_msec (required for events to show in real-time)
            payload.Events[0].Params["engagement_time_msec"] = 100;

            var url = _options.DebugMode ? DebugUrl : BaseUrl;
            url = $"{url}?measurement_id={_options.MeasurementId}&api_secret={_options.ApiSecret}";

            var response = await _httpClient.PostAsJsonAsync(url, payload);

            if (_options.DebugMode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var validation = JsonSerializer.Deserialize<GA4ValidationResponse>(content);

                if (validation?.ValidationMessages?.Any() == true)
                {
                    foreach (var msg in validation.ValidationMessages)
                    {
                        _logger.LogWarning("[GoogleConversions] Validation: {Field} - {Description}",
                            msg.FieldPath, msg.Description);
                    }
                    return false;
                }
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[GoogleConversions] Event sent: {EventName}", eventName);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("[GoogleConversions] Error: {StatusCode} - {Content}",
                response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GoogleConversions] Failed to send event: {EventName}", eventName);
            return false;
        }
    }
}
