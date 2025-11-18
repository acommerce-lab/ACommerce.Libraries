namespace ACommerce.Authentication.Abstractions;

public record TwoFactorVerificationRequest
{
	public required string TransactionId { get; init; }
	public string? VerificationCode { get; init; }
	public Dictionary<string, string>? Metadata { get; init; }
}

