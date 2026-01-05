using System.Net.Http.Json;
using System.Text.Json;
using ACommerce.Marketing.TwitterConversions.Helpers;
using ACommerce.Marketing.TwitterConversions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.Marketing.TwitterConversions.Services;

public class TwitterConversionsService : ITwitterConversionsService
{
    private readonly HttpClient _httpClient;
    private readonly TwitterConversionsOptions _options;
    private readonly ILogger<TwitterConversionsService> _logger;

    private const string BaseUrl = "https://ads-api.twitter.com/12/measurement/conversions";

    public TwitterConversionsService(
        HttpClient httpClient,
        IOptions<TwitterConversionsOptions> options,
        ILogger<TwitterConversionsService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> TrackPurchaseAsync(TwitterPurchaseEventRequest request)
    {
        var conversion = CreateBaseConversion("tw-purchase", request.User);
        conversion.ConversionId = request.TransactionId;
        conversion.Value = request.Value.ToString("F2");
        conversion.PriceCurrency = request.Currency;
        conversion.NumberItems = request.NumberItems;
        conversion.Contents = request.Contents;

        return await SendEventAsync(conversion);
    }

    public async Task<bool> TrackViewContentAsync(TwitterViewContentEventRequest request)
    {
        var conversion = CreateBaseConversion("tw-viewcontent", request.User);
        conversion.Contents = new List<TwitterContent>
        {
            new()
            {
                ContentId = request.ContentId,
                ContentName = request.ContentName,
                ContentType = request.ContentType ?? "product",
                ContentPrice = request.Value?.ToString("F2")
            }
        };

        return await SendEventAsync(conversion);
    }

    public async Task<bool> TrackSearchAsync(TwitterSearchEventRequest request)
    {
        var conversion = CreateBaseConversion("tw-search", request.User);
        conversion.SearchString = request.SearchQuery;

        return await SendEventAsync(conversion);
    }

    public async Task<bool> TrackRegistrationAsync(TwitterRegistrationEventRequest request)
    {
        var conversion = CreateBaseConversion("tw-signup", request.User);
        return await SendEventAsync(conversion);
    }

    public async Task<bool> TrackLoginAsync(TwitterLoginEventRequest request)
    {
        var conversion = CreateBaseConversion("tw-login", request.User);
        return await SendEventAsync(conversion);
    }

    public async Task<bool> TrackAddToWishlistAsync(TwitterAddToWishlistEventRequest request)
    {
        var conversion = CreateBaseConversion("tw-addtowishlist", request.User);
        conversion.Contents = new List<TwitterContent>
        {
            new()
            {
                ContentId = request.ContentId,
                ContentName = request.ContentName,
                ContentPrice = request.Value?.ToString("F2")
            }
        };

        return await SendEventAsync(conversion);
    }

    public async Task<bool> TrackCustomEventAsync(TwitterCustomEventRequest request)
    {
        var conversion = CreateBaseConversion(request.EventId, request.User);
        conversion.Description = request.Description;

        return await SendEventAsync(conversion);
    }

    private TwitterConversion CreateBaseConversion(string eventId, TwitterUserContext? user)
    {
        var conversion = new TwitterConversion
        {
            ConversionTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            EventId = eventId,
            Identifiers = new List<TwitterIdentifier>()
        };

        if (user != null)
        {
            var identifier = new TwitterIdentifier
            {
                HashedEmail = HashHelper.HashEmail(user.Email),
                HashedPhoneNumber = HashHelper.HashPhone(user.Phone),
                Twclid = user.Twclid
            };

            // Only add if at least one identifier is present
            if (!string.IsNullOrEmpty(identifier.HashedEmail) ||
                !string.IsNullOrEmpty(identifier.HashedPhoneNumber) ||
                !string.IsNullOrEmpty(identifier.Twclid))
            {
                conversion.Identifiers.Add(identifier);
            }
        }

        return conversion;
    }

    private async Task<bool> SendEventAsync(TwitterConversion conversion)
    {
        if (!_options.IsEnabled)
        {
            _logger.LogWarning("[TwitterConversions] Service not configured. Skipping event: {EventId}", conversion.EventId);
            return false;
        }

        try
        {
            var url = $"{BaseUrl}/{_options.PixelId}";

            var payload = new TwitterEventPayload
            {
                Conversions = new List<TwitterConversion> { conversion }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", $"Bearer {_options.AccessToken}");
            request.Content = JsonContent.Create(payload);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<TwitterEventResponse>(content);
                if (result?.Data?.ConversionsProcessed > 0)
                {
                    _logger.LogInformation("[TwitterConversions] Event sent: {EventId}", conversion.EventId);
                    return true;
                }

                if (result?.Errors?.Any() == true)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogWarning("[TwitterConversions] API Error: {Code} - {Message}",
                            error.Code, error.Message);
                    }
                }
                return false;
            }

            _logger.LogError("[TwitterConversions] HTTP Error: {StatusCode} - {Content}",
                response.StatusCode, content);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TwitterConversions] Failed to send event: {EventId}", conversion.EventId);
            return false;
        }
    }
}
