namespace ACommerce.Authentication.Abstractions;

public record AuthenticationResult
{
	public required bool Success { get; init; }
	public string? AccessToken { get; init; }
	public string? RefreshToken { get; init; }
	public string? TokenType { get; init; } = "Bearer";
	public DateTimeOffset? ExpiresAt { get; init; }
	public string? UserId { get; init; }
	public AuthenticationError? Error { get; init; }
	public Dictionary<string, object>? Metadata { get; init; }
}

