namespace ACommerce.Realtime.Abstractions.Models;

public record RealtimeMessage
{
	public required string Method { get; init; }
	public required object Data { get; init; }
	public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
	public Dictionary<string, string>? Metadata { get; init; }
}

