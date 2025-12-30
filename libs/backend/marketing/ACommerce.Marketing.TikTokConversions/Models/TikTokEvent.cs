using System.Text.Json.Serialization;

namespace ACommerce.Marketing.TikTokConversions.Models;

/// <summary>
/// TikTok Events API Payload
/// </summary>
public class TikTokEventPayload
{
    [JsonPropertyName("pixel_code")]
    public string PixelCode { get; set; } = string.Empty;

    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("context")]
    public TikTokContext Context { get; set; } = new();

    [JsonPropertyName("properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TikTokProperties? Properties { get; set; }

    [JsonPropertyName("test_event_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TestEventCode { get; set; }
}

public class TikTokContext
{
    [JsonPropertyName("user")]
    public TikTokUser User { get; set; } = new();

    [JsonPropertyName("ad")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TikTokAd? Ad { get; set; }

    [JsonPropertyName("page")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TikTokPage? Page { get; set; }

    [JsonPropertyName("user_agent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserAgent { get; set; }

    [JsonPropertyName("ip")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Ip { get; set; }
}

public class TikTokUser
{
    [JsonPropertyName("external_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExternalId { get; set; }

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonPropertyName("phone_number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("ttp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Ttp { get; set; }

    [JsonPropertyName("ttclid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Ttclid { get; set; }
}

public class TikTokAd
{
    [JsonPropertyName("callback")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Callback { get; set; }
}

public class TikTokPage
{
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; set; }

    [JsonPropertyName("referrer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Referrer { get; set; }
}

public class TikTokProperties
{
    [JsonPropertyName("contents")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<TikTokContent>? Contents { get; set; }

    [JsonPropertyName("currency")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Currency { get; set; }

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal Value { get; set; }

    [JsonPropertyName("query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Query { get; set; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonPropertyName("order_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OrderId { get; set; }
}

public class TikTokContent
{
    [JsonPropertyName("content_id")]
    public string ContentId { get; set; } = string.Empty;

    [JsonPropertyName("content_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentName { get; set; }

    [JsonPropertyName("content_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentType { get; set; }

    [JsonPropertyName("content_category")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentCategory { get; set; }

    [JsonPropertyName("price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal Price { get; set; }

    [JsonPropertyName("quantity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// User context for TikTok events
/// </summary>
public class TikTokUserContext
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Ttclid { get; set; }
    public string? Ttp { get; set; }
    public string? PageUrl { get; set; }
    public string? Referrer { get; set; }
}

/// <summary>
/// Event request models
/// </summary>
public class TikTokPurchaseEventRequest
{
    public TikTokUserContext? User { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Currency { get; set; } = "SAR";
    public List<TikTokContent>? Contents { get; set; }
}

public class TikTokViewContentEventRequest
{
    public TikTokUserContext? User { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string? ContentName { get; set; }
    public string? ContentType { get; set; }
    public string? Category { get; set; }
    public decimal? Value { get; set; }
}

public class TikTokSearchEventRequest
{
    public TikTokUserContext? User { get; set; }
    public string SearchQuery { get; set; } = string.Empty;
}

public class TikTokRegistrationEventRequest
{
    public TikTokUserContext? User { get; set; }
}

public class TikTokLoginEventRequest
{
    public TikTokUserContext? User { get; set; }
}

public class TikTokAddToWishlistEventRequest
{
    public TikTokUserContext? User { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string? ContentName { get; set; }
    public string? ContentType { get; set; }
    public decimal? Value { get; set; }
}

public class TikTokCustomEventRequest
{
    public TikTokUserContext? User { get; set; }
    public string EventName { get; set; } = string.Empty;
    public TikTokProperties? Properties { get; set; }
}
