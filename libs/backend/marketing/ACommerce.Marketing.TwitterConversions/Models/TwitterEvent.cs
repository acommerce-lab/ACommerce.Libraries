using System.Text.Json.Serialization;

namespace ACommerce.Marketing.TwitterConversions.Models;

/// <summary>
/// Twitter/X Conversions API Event Payload
/// </summary>
public class TwitterEventPayload
{
    [JsonPropertyName("conversions")]
    public List<TwitterConversion> Conversions { get; set; } = new();
}

public class TwitterConversion
{
    [JsonPropertyName("conversion_time")]
    public string ConversionTime { get; set; } = string.Empty;

    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("identifiers")]
    public List<TwitterIdentifier> Identifiers { get; set; } = new();

    [JsonPropertyName("conversion_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ConversionId { get; set; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonPropertyName("number_items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int NumberItems { get; set; }

    [JsonPropertyName("price_currency")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PriceCurrency { get; set; }

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Value { get; set; }

    [JsonPropertyName("contents")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<TwitterContent>? Contents { get; set; }

    [JsonPropertyName("search_string")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SearchString { get; set; }
}

public class TwitterIdentifier
{
    [JsonPropertyName("hashed_email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HashedEmail { get; set; }

    [JsonPropertyName("hashed_phone_number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HashedPhoneNumber { get; set; }

    [JsonPropertyName("twclid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Twclid { get; set; }
}

public class TwitterContent
{
    [JsonPropertyName("content_id")]
    public string ContentId { get; set; } = string.Empty;

    [JsonPropertyName("content_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentName { get; set; }

    [JsonPropertyName("content_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentType { get; set; }

    [JsonPropertyName("content_price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentPrice { get; set; }

    [JsonPropertyName("num_items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int NumItems { get; set; } = 1;
}

/// <summary>
/// User context for Twitter events
/// </summary>
public class TwitterUserContext
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Twclid { get; set; }
}

/// <summary>
/// Event request models
/// </summary>
public class TwitterPurchaseEventRequest
{
    public TwitterUserContext? User { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Currency { get; set; } = "SAR";
    public List<TwitterContent>? Contents { get; set; }
    public int NumberItems { get; set; } = 1;
}

public class TwitterViewContentEventRequest
{
    public TwitterUserContext? User { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string? ContentName { get; set; }
    public string? ContentType { get; set; }
    public decimal? Value { get; set; }
}

public class TwitterSearchEventRequest
{
    public TwitterUserContext? User { get; set; }
    public string SearchQuery { get; set; } = string.Empty;
}

public class TwitterRegistrationEventRequest
{
    public TwitterUserContext? User { get; set; }
}

public class TwitterLoginEventRequest
{
    public TwitterUserContext? User { get; set; }
}

public class TwitterAddToWishlistEventRequest
{
    public TwitterUserContext? User { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string? ContentName { get; set; }
    public decimal? Value { get; set; }
}

public class TwitterCustomEventRequest
{
    public TwitterUserContext? User { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string? Description { get; set; }
}
