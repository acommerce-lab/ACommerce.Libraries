using System.Net.Http.Json;
using System.Text.Json;
using ACommerce.Marketing.MetaConversions.Helpers;
using ACommerce.Marketing.MetaConversions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.Marketing.MetaConversions.Services;

public class MetaConversionsService : IMetaConversionsService
{
    private readonly HttpClient _httpClient;
    private readonly MetaConversionsOptions _options;
    private readonly ILogger<MetaConversionsService> _logger;

    public MetaConversionsService(
        HttpClient httpClient,
        IOptions<MetaConversionsOptions> options,
        ILogger<MetaConversionsService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> TrackPurchaseAsync(PurchaseEventRequest request)
    {
        var conversionEvent = new ConversionEvent
        {
            EventName = "Purchase",
            EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            EventId = request.TransactionId,
            ActionSource = "website",
            UserData = CreateUserData(request.User),
            CustomData = new CustomData
            {
                Currency = request.Currency,
                Value = request.Value,
                ContentName = request.ContentName,
                ContentIds = request.ContentIds,
                NumItems = request.NumItems,
                ContentType = "product"
            }        };

        var response = await SendEventAsync(conversionEvent);
        return response?.EventsReceived > 0;
    }

    public async Task<bool> TrackViewContentAsync(ViewContentEventRequest request)
    {
        var conversionEvent = new ConversionEvent
        {
            EventName = "ViewContent",
            EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            EventId = $"vc_{request.ContentId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            ActionSource = "website",
            UserData = CreateUserData(request.User),
            CustomData = new CustomData
            {
                ContentIds = new[] { request.ContentId },
                ContentName = request.ContentName,
                ContentType = request.ContentType ?? "listing",
                ContentCategory = request.Category,
                Value = request.Value ?? 0
            }        };

        var response = await SendEventAsync(conversionEvent);
        return response?.EventsReceived > 0;
    }

    public async Task<bool> TrackSearchAsync(SearchEventRequest request)
    {
        var conversionEvent = new ConversionEvent
        {
            EventName = "Search",
            EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            EventId = $"search_{Guid.NewGuid():N}",
            ActionSource = "website",
            UserData = CreateUserData(request.User),
            CustomData = new CustomData
            {
                SearchString = request.SearchQuery,
                NumItems = request.ResultsCount ?? 0
            }        };

        var response = await SendEventAsync(conversionEvent);
        return response?.EventsReceived > 0;
    }

    public async Task<bool> TrackRegistrationAsync(RegistrationEventRequest request)
    {
        var conversionEvent = new ConversionEvent
        {
            EventName = "CompleteRegistration",
            EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            EventId = $"reg_{request.User?.UserId ?? Guid.NewGuid().ToString("N")}",
            ActionSource = "website",
            UserData = CreateUserData(request.User)        };

        var response = await SendEventAsync(conversionEvent);
        return response?.EventsReceived > 0;
    }

    public async Task<bool> TrackLoginAsync(LoginEventRequest request)
    {
        var conversionEvent = new ConversionEvent
        {
            EventName = "Login",
            EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            EventId = $"login_{request.User?.UserId ?? Guid.NewGuid().ToString("N")}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            ActionSource = "website",
            UserData = CreateUserData(request.User)        };

        var response = await SendEventAsync(conversionEvent);
        return response?.EventsReceived > 0;
    }

    public async Task<bool> TrackAddToWishlistAsync(AddToWishlistEventRequest request)
    {
        var conversionEvent = new ConversionEvent
        {
            EventName = "AddToWishlist",
            EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            EventId = $"wish_{request.ContentId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            ActionSource = "website",
            UserData = CreateUserData(request.User),
            CustomData = new CustomData
            {
                ContentIds = new[] { request.ContentId },
                ContentName = request.ContentName,
                ContentType = request.ContentType ?? "listing",
                Value = request.Value ?? 0
            }        };

        var response = await SendEventAsync(conversionEvent);
        return response?.EventsReceived > 0;
    }

    public async Task<bool> TrackCustomEventAsync(CustomEventRequest request)
    {
        var conversionEvent = new ConversionEvent
        {
            EventName = request.EventName,
            EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            EventId = $"custom_{Guid.NewGuid():N}",
            ActionSource = "website",
            UserData = CreateUserData(request.User)        };

        var response = await SendEventAsync(conversionEvent);
        return response?.EventsReceived > 0;
    }

    public async Task<ConversionResponse?> SendEventAsync(ConversionEvent conversionEvent)
    {
        if (!_options.IsEnabled)
        {
            _logger.LogWarning("[MetaConversions] Service not configured. Skipping event: {EventName}", conversionEvent.EventName);
            return null;
        }

        try
        {
            var url = $"https://graph.facebook.com/{_options.ApiVersion}/{_options.PixelId}/events";

            var payload = new
            {
                data = new[] { conversionEvent },
                access_token = _options.AccessToken,
                test_event_code = _options.TestEventCode
            };

            var response = await _httpClient.PostAsJsonAsync(url, payload);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ConversionResponse>(content);
                _logger.LogInformation("[MetaConversions] Event sent: {EventName}, Received: {Count}",
                    conversionEvent.EventName, result?.EventsReceived);
                return result;
            }

            _logger.LogError("[MetaConversions] Error Response: {Content}", content);
            var error = JsonSerializer.Deserialize<ConversionErrorResponse>(content);
            _logger.LogError("[MetaConversions] Error: {Message}, Code: {Code}, Type: {Type}, Subcode: {Subcode}",
                error?.Error?.Message,
                error?.Error?.Code,
                error?.Error?.Type,
                error?.Error?.ErrorSubcode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MetaConversions] Failed to send event: {EventName}", conversionEvent.EventName);
            return null;
        }
    }

    private UserData CreateUserData(UserContext? user)
    {
        if (user == null)
            return new UserData();

        return new UserData
        {
            ExternalId = HashHelper.Sha256Hash(user.UserId),
            Email = HashHelper.HashEmail(user.Email),
            Phone = HashHelper.HashPhone(user.Phone),
            FirstName = HashHelper.Sha256Hash(user.FirstName),
            LastName = HashHelper.Sha256Hash(user.LastName),
            City = HashHelper.Sha256Hash(user.City),
            Country = HashHelper.Sha256Hash(user.Country ?? "sa"),
            ClientIpAddress = user.IpAddress,
            ClientUserAgent = user.UserAgent,
            Fbc = user.Fbc,
            Fbp = user.Fbp
        };
    }
}
