namespace ACommerce.Authentication.Abstractions;

public record TwoFactorInitiationResult
{
	public required bool Success { get; init; }
	public string? TransactionId { get; init; }
	public string? VerificationCode { get; init; }
	public string? Message { get; init; }
	public TimeSpan? ExpiresIn { get; init; }
	public TwoFactorError? Error { get; init; }
}

