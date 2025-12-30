using System.Text.Json.Serialization;

namespace ACommerce.Marketing.TikTokConversions.Models;

public class TikTokEventResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public TikTokEventData? Data { get; set; }
}

public class TikTokEventData
{
    [JsonPropertyName("events_received")]
    public int EventsReceived { get; set; }
}
