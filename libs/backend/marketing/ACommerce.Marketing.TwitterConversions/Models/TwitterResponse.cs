using System.Text.Json.Serialization;

namespace ACommerce.Marketing.TwitterConversions.Models;

public class TwitterEventResponse
{
    [JsonPropertyName("data")]
    public TwitterResponseData? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<TwitterError>? Errors { get; set; }
}

public class TwitterResponseData
{
    [JsonPropertyName("conversions_processed")]
    public int ConversionsProcessed { get; set; }
}

public class TwitterError
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
