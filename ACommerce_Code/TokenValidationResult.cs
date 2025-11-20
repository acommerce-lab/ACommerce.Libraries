namespace ACommerce.Authentication.Abstractions;

public record TokenValidationResult
{
	public required bool IsValid { get; init; }
	public string? UserId { get; init; }
	public IReadOnlyDictionary<string, string>? Claims { get; init; }
	public string? Error { get; init; }
}

