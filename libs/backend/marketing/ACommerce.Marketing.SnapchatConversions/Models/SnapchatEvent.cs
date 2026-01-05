using System.Text.Json.Serialization;

namespace ACommerce.Marketing.SnapchatConversions.Models;

/// <summary>
/// Snapchat Conversions API Event Payload
/// </summary>
public class SnapchatEventPayload
{
    [JsonPropertyName("pixel_id")]
    public string PixelId { get; set; } = string.Empty;

    [JsonPropertyName("event_type")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("event_conversion_type")]
    public string EventConversionType { get; set; } = "WEB";

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("hashed_email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HashedEmail { get; set; }

    [JsonPropertyName("hashed_phone_number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HashedPhoneNumber { get; set; }

    [JsonPropertyName("user_agent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserAgent { get; set; }

    [JsonPropertyName("hashed_ip_address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HashedIpAddress { get; set; }

    [JsonPropertyName("uuid_c1")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UuidC1 { get; set; }

    [JsonPropertyName("sc_click_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ScClickId { get; set; }

    [JsonPropertyName("page_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PageUrl { get; set; }

    [JsonPropertyName("item_ids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ItemIds { get; set; }

    [JsonPropertyName("item_category")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ItemCategory { get; set; }

    [JsonPropertyName("price")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal Price { get; set; }

    [JsonPropertyName("currency")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Currency { get; set; }

    [JsonPropertyName("transaction_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TransactionId { get; set; }

    [JsonPropertyName("number_items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int NumberItems { get; set; }

    [JsonPropertyName("search_string")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SearchString { get; set; }

    [JsonPropertyName("sign_up_method")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SignUpMethod { get; set; }

    [JsonPropertyName("event_tag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EventTag { get; set; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
}

/// <summary>
/// User context for Snapchat events
/// </summary>
public class SnapchatUserContext
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ScClickId { get; set; }
    public string? UuidC1 { get; set; }
    public string? PageUrl { get; set; }
}

/// <summary>
/// Event request models
/// </summary>
public class SnapchatPurchaseEventRequest
{
    public SnapchatUserContext? User { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Currency { get; set; } = "SAR";
    public List<string>? ItemIds { get; set; }
    public int NumberItems { get; set; } = 1;
}

public class SnapchatViewContentEventRequest
{
    public SnapchatUserContext? User { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string? ContentName { get; set; }
    public string? Category { get; set; }
    public decimal? Value { get; set; }
}

public class SnapchatSearchEventRequest
{
    public SnapchatUserContext? User { get; set; }
    public string SearchQuery { get; set; } = string.Empty;
}

public class SnapchatRegistrationEventRequest
{
    public SnapchatUserContext? User { get; set; }
    public string? Method { get; set; }
}

public class SnapchatLoginEventRequest
{
    public SnapchatUserContext? User { get; set; }
}

public class SnapchatAddToWishlistEventRequest
{
    public SnapchatUserContext? User { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string? ContentName { get; set; }
    public decimal? Value { get; set; }
}

public class SnapchatCustomEventRequest
{
    public SnapchatUserContext? User { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? EventTag { get; set; }
}
