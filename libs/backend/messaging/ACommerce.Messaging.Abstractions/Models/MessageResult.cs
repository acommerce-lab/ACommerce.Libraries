namespace ACommerce.Messaging.Abstractions.Models;

/// <summary>
/// Result of publish operation
/// </summary>
public record MessageResult
{
    public bool Success { get; init; }
    public string? MessageId { get; init; }
    public string? Error { get; init; }
    public Dictionary<string, object>? Details { get; init; }

    public static MessageResult Ok(string messageId, int subscriberCount = 0) => new()
    {
        Success = true,
        MessageId = messageId,
        Details = new Dictionary<string, object>
        {
            ["subscriberCount"] = subscriberCount
        }
    };

    public static MessageResult Fail(string error) => new()
    {
        Success = false,
        Error = error
    };
}