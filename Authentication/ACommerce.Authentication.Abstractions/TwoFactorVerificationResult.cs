namespace ACommerce.Authentication.Abstractions;

public record TwoFactorVerificationResult
{
	public required bool Success { get; init; }
	public string? UserId { get; init; }
	public IReadOnlyDictionary<string, string>? UserClaims { get; init; }
	public TwoFactorError? Error { get; init; }
}

