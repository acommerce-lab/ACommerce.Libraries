using System.Text.Json.Serialization;

namespace ACommerce.Marketing.SnapchatConversions.Models;

public class SnapchatEventResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public class SnapchatBatchResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("results")]
    public List<SnapchatEventResponse>? Results { get; set; }
}
