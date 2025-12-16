using System.Text.Json.Serialization;

namespace ACommerce.Marketing.MetaConversions.Models;

public class ConversionResponse
{
    [JsonPropertyName("events_received")]
    public int EventsReceived { get; set; }

    [JsonPropertyName("messages")]
    public string[]? Messages { get; set; }

    [JsonPropertyName("fbtrace_id")]
    public string? FbtraceId { get; set; }
}

public class ConversionErrorResponse
{
    [JsonPropertyName("error")]
    public ConversionError? Error { get; set; }
}

public class ConversionError
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("fbtrace_id")]
    public string? FbtraceId { get; set; }
}
