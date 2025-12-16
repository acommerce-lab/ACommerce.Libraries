using System.Text.Json.Serialization;

namespace ACommerce.Marketing.MetaConversions.Models;

public class ConversionEvent
{
    [JsonPropertyName("event_name")]
    public string EventName { get; set; } = string.Empty;

    [JsonPropertyName("event_time")]
    public long EventTime { get; set; }

    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("event_source_url")]
    public string? EventSourceUrl { get; set; }

    [JsonPropertyName("action_source")]
    public string ActionSource { get; set; } = "app";

    [JsonPropertyName("user_data")]
    public UserData UserData { get; set; } = new();

    [JsonPropertyName("custom_data")]
    public CustomData? CustomData { get; set; }

    [JsonPropertyName("app_data")]
    public AppData? AppData { get; set; }
}

public class UserData
{
    [JsonPropertyName("em")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonPropertyName("ph")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Phone { get; set; }

    [JsonPropertyName("fn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FirstName { get; set; }

    [JsonPropertyName("ln")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastName { get; set; }

    [JsonPropertyName("ct")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Country { get; set; }

    [JsonPropertyName("external_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExternalId { get; set; }

    [JsonPropertyName("client_ip_address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClientIpAddress { get; set; }

    [JsonPropertyName("client_user_agent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClientUserAgent { get; set; }

    [JsonPropertyName("fbc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Fbc { get; set; }

    [JsonPropertyName("fbp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Fbp { get; set; }
}

public class CustomData
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "SAR";

    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("content_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentName { get; set; }

    [JsonPropertyName("content_ids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? ContentIds { get; set; }

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = "product";

    [JsonPropertyName("num_items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int NumItems { get; set; }

    [JsonPropertyName("search_string")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SearchString { get; set; }

    [JsonPropertyName("content_category")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentCategory { get; set; }
}

public class AppData
{
    [JsonPropertyName("application_tracking_enabled")]
    public bool ApplicationTrackingEnabled { get; set; } = true;

    [JsonPropertyName("advertiser_tracking_enabled")]
    public bool AdvertiserTrackingEnabled { get; set; } = true;

    [JsonPropertyName("extinfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? ExtInfo { get; set; }
}
