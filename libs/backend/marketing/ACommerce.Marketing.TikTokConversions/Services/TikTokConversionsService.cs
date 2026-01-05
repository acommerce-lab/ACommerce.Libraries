using System.Net.Http.Json;
using System.Text.Json;
using ACommerce.Marketing.TikTokConversions.Helpers;
using ACommerce.Marketing.TikTokConversions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.Marketing.TikTokConversions.Services;

public class TikTokConversionsService : ITikTokConversionsService
{
    private readonly HttpClient _httpClient;
    private readonly TikTokConversionsOptions _options;
    private readonly ILogger<TikTokConversionsService> _logger;

    private const string BaseUrl = "https://business-api.tiktok.com/open_api/v1.3/pixel/track/";

    public TikTokConversionsService(
        HttpClient httpClient,
        IOptions<TikTokConversionsOptions> options,
        ILogger<TikTokConversionsService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> TrackPurchaseAsync(TikTokPurchaseEventRequest request)
    {
        var payload = CreateBasePayload("CompletePayment", request.User);
        payload.Properties = new TikTokProperties
        {
            Currency = request.Currency,
            Value = request.Value,
            OrderId = request.TransactionId,
            Contents = request.Contents ?? new List<TikTokContent>()
        };

        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackViewContentAsync(TikTokViewContentEventRequest request)
    {
        var payload = CreateBasePayload("ViewContent", request.User);
        payload.Properties = new TikTokProperties
        {
            Contents = new List<TikTokContent>
            {
                new()
                {
                    ContentId = request.ContentId,
                    ContentName = request.ContentName,
                    ContentType = request.ContentType ?? "product",
                    ContentCategory = request.Category,
                    Price = request.Value ?? 0
                }
            }
        };

        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackSearchAsync(TikTokSearchEventRequest request)
    {
        var payload = CreateBasePayload("Search", request.User);
        payload.Properties = new TikTokProperties
        {
            Query = request.SearchQuery
        };

        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackRegistrationAsync(TikTokRegistrationEventRequest request)
    {
        var payload = CreateBasePayload("CompleteRegistration", request.User);
        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackLoginAsync(TikTokLoginEventRequest request)
    {
        var payload = CreateBasePayload("Login", request.User);
        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackAddToWishlistAsync(TikTokAddToWishlistEventRequest request)
    {
        var payload = CreateBasePayload("AddToWishlist", request.User);
        payload.Properties = new TikTokProperties
        {
            Contents = new List<TikTokContent>
            {
                new()
                {
                    ContentId = request.ContentId,
                    ContentName = request.ContentName,
                    ContentType = request.ContentType ?? "product",
                    Price = request.Value ?? 0
                }
            }
        };

        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackCustomEventAsync(TikTokCustomEventRequest request)
    {
        var payload = CreateBasePayload(request.EventName, request.User);
        payload.Properties = request.Properties;
        return await SendEventAsync(payload);
    }

    private TikTokEventPayload CreateBasePayload(string eventName, TikTokUserContext? user)
    {
        var payload = new TikTokEventPayload
        {
            PixelCode = _options.PixelCode,
            Event = eventName,
            EventId = $"{eventName.ToLower()}_{Guid.NewGuid():N}",
            Timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            TestEventCode = _options.TestEventCode
        };

        if (user != null)
        {
            payload.Context = new TikTokContext
            {
                User = new TikTokUser
                {
                    ExternalId = HashHelper.Sha256Hash(user.UserId),
                    Email = HashHelper.HashEmail(user.Email),
                    PhoneNumber = HashHelper.HashPhone(user.Phone),
                    Ttclid = user.Ttclid,
                    Ttp = user.Ttp
                },
                Ip = user.IpAddress,
                UserAgent = user.UserAgent,
                Page = !string.IsNullOrEmpty(user.PageUrl) ? new TikTokPage
                {
                    Url = user.PageUrl,
                    Referrer = user.Referrer
                } : null
            };
        }

        return payload;
    }

    private async Task<bool> SendEventAsync(TikTokEventPayload payload)
    {
        if (!_options.IsEnabled)
        {
            _logger.LogWarning("[TikTokConversions] Service not configured. Skipping event: {Event}", payload.Event);
            return false;
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
            request.Headers.Add("Access-Token", _options.AccessToken);
            request.Content = JsonContent.Create(new { data = new[] { payload } });

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<TikTokEventResponse>(content);
                if (result?.Code == 0)
                {
                    _logger.LogInformation("[TikTokConversions] Event sent: {Event}", payload.Event);
                    return true;
                }

                _logger.LogWarning("[TikTokConversions] API Error: {Code} - {Message}",
                    result?.Code, result?.Message);
                return false;
            }

            _logger.LogError("[TikTokConversions] HTTP Error: {StatusCode} - {Content}",
                response.StatusCode, content);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TikTokConversions] Failed to send event: {Event}", payload.Event);
            return false;
        }
    }
}
