using System.Net.Http.Json;
using System.Text.Json;
using ACommerce.Marketing.SnapchatConversions.Helpers;
using ACommerce.Marketing.SnapchatConversions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.Marketing.SnapchatConversions.Services;

public class SnapchatConversionsService : ISnapchatConversionsService
{
    private readonly HttpClient _httpClient;
    private readonly SnapchatConversionsOptions _options;
    private readonly ILogger<SnapchatConversionsService> _logger;

    private const string BaseUrl = "https://tr.snapchat.com/v2/conversion";

    public SnapchatConversionsService(
        HttpClient httpClient,
        IOptions<SnapchatConversionsOptions> options,
        ILogger<SnapchatConversionsService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> TrackPurchaseAsync(SnapchatPurchaseEventRequest request)
    {
        var payload = CreateBasePayload("PURCHASE", request.User);
        payload.TransactionId = request.TransactionId;
        payload.Price = request.Value;
        payload.Currency = request.Currency;
        payload.ItemIds = request.ItemIds;
        payload.NumberItems = request.NumberItems;

        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackViewContentAsync(SnapchatViewContentEventRequest request)
    {
        var payload = CreateBasePayload("VIEW_CONTENT", request.User);
        payload.ItemIds = new List<string> { request.ContentId };
        payload.ItemCategory = request.Category;
        payload.Description = request.ContentName;
        if (request.Value.HasValue)
            payload.Price = request.Value.Value;

        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackSearchAsync(SnapchatSearchEventRequest request)
    {
        var payload = CreateBasePayload("SEARCH", request.User);
        payload.SearchString = request.SearchQuery;

        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackRegistrationAsync(SnapchatRegistrationEventRequest request)
    {
        var payload = CreateBasePayload("SIGN_UP", request.User);
        payload.SignUpMethod = request.Method;

        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackLoginAsync(SnapchatLoginEventRequest request)
    {
        var payload = CreateBasePayload("LOGIN", request.User);
        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackAddToWishlistAsync(SnapchatAddToWishlistEventRequest request)
    {
        var payload = CreateBasePayload("ADD_TO_WISHLIST", request.User);
        payload.ItemIds = new List<string> { request.ContentId };
        payload.Description = request.ContentName;
        if (request.Value.HasValue)
            payload.Price = request.Value.Value;

        return await SendEventAsync(payload);
    }

    public async Task<bool> TrackCustomEventAsync(SnapchatCustomEventRequest request)
    {
        var payload = CreateBasePayload(request.EventType, request.User);
        payload.Description = request.Description;
        payload.EventTag = request.EventTag;

        return await SendEventAsync(payload);
    }

    private SnapchatEventPayload CreateBasePayload(string eventType, SnapchatUserContext? user)
    {
        var payload = new SnapchatEventPayload
        {
            PixelId = _options.PixelId,
            EventType = eventType,
            EventConversionType = "WEB",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        if (user != null)
        {
            payload.HashedEmail = HashHelper.HashEmail(user.Email);
            payload.HashedPhoneNumber = HashHelper.HashPhone(user.Phone);
            payload.HashedIpAddress = HashHelper.Sha256Hash(user.IpAddress);
            payload.UserAgent = user.UserAgent;
            payload.ScClickId = user.ScClickId;
            payload.UuidC1 = user.UuidC1;
            payload.PageUrl = user.PageUrl;
        }

        return payload;
    }

    private async Task<bool> SendEventAsync(SnapchatEventPayload payload)
    {
        if (!_options.IsEnabled)
        {
            _logger.LogWarning("[SnapchatConversions] Service not configured. Skipping event: {EventType}", payload.EventType);
            return false;
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
            request.Headers.Add("Authorization", $"Bearer {_options.AccessToken}");
            request.Content = JsonContent.Create(new[] { payload });

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[SnapchatConversions] Event sent: {EventType}", payload.EventType);
                return true;
            }

            _logger.LogError("[SnapchatConversions] HTTP Error: {StatusCode} - {Content}",
                response.StatusCode, content);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SnapchatConversions] Failed to send event: {EventType}", payload.EventType);
            return false;
        }
    }
}
