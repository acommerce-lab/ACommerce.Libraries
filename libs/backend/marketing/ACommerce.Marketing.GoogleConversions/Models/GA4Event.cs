using System.Text.Json.Serialization;

namespace ACommerce.Marketing.GoogleConversions.Models;

/// <summary>
/// GA4 Measurement Protocol Event Payload
/// </summary>
public class GA4EventPayload
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserId { get; set; }

    [JsonPropertyName("timestamp_micros")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long TimestampMicros { get; set; }

    [JsonPropertyName("user_properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, GA4Value>? UserProperties { get; set; }

    [JsonPropertyName("events")]
    public List<GA4Event> Events { get; set; } = new();
}

public class GA4Event
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public Dictionary<string, object> Params { get; set; } = new();
}

public class GA4Value
{
    [JsonPropertyName("value")]
    public object Value { get; set; } = null!;
}

/// <summary>
/// User context for Google events
/// </summary>
public class GoogleUserContext
{
    public string? UserId { get; set; }
    public string? ClientId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Gclid { get; set; }
    public string? SessionId { get; set; }
}

/// <summary>
/// Event request models
/// </summary>
public class GooglePurchaseEventRequest
{
    public GoogleUserContext? User { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? Coupon { get; set; }
    public List<GoogleItem>? Items { get; set; }
}

public class GoogleItem
{
    public string ItemId { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public string? ItemCategory { get; set; }
    public decimal? Price { get; set; }
    public int Quantity { get; set; } = 1;
}

public class GoogleViewContentEventRequest
{
    public GoogleUserContext? User { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string? ContentName { get; set; }
    public string? ContentType { get; set; }
    public string? Category { get; set; }
    public decimal? Value { get; set; }
}

public class GoogleSearchEventRequest
{
    public GoogleUserContext? User { get; set; }
    public string SearchQuery { get; set; } = string.Empty;
}

public class GoogleRegistrationEventRequest
{
    public GoogleUserContext? User { get; set; }
    public string? Method { get; set; }
}

public class GoogleLoginEventRequest
{
    public GoogleUserContext? User { get; set; }
    public string? Method { get; set; }
}

public class GoogleAddToWishlistEventRequest
{
    public GoogleUserContext? User { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string? ContentName { get; set; }
    public string? ContentType { get; set; }
    public decimal? Value { get; set; }
}

public class GoogleCustomEventRequest
{
    public GoogleUserContext? User { get; set; }
    public string EventName { get; set; } = string.Empty;
    public Dictionary<string, object>? Params { get; set; }
}
