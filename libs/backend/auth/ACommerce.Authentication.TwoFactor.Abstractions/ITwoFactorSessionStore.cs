namespace ACommerce.Authentication.TwoFactor.Abstractions;

/// <summary>
/// Store for managing two-factor authentication sessions
/// </summary>
public interface ITwoFactorSessionStore
{
	Task<string> CreateSessionAsync(
		TwoFactorSession session,
		CancellationToken cancellationToken = default);

	Task<TwoFactorSession?> GetSessionAsync(
		string transactionId,
		CancellationToken cancellationToken = default);

	Task UpdateSessionAsync(
		TwoFactorSession session,
		CancellationToken cancellationToken = default);

	Task DeleteSessionAsync(
		string transactionId,
		CancellationToken cancellationToken = default);
}

public record TwoFactorSession
{
	public required string TransactionId { get; init; }
	public required string Identifier { get; init; }
	public required string Provider { get; init; }
	public required DateTimeOffset CreatedAt { get; init; }
	public required DateTimeOffset ExpiresAt { get; init; }
	public string? VerificationCode { get; init; }
	public TwoFactorSessionStatus Status { get; init; }
	public Dictionary<string, string>? Metadata { get; init; }
}

public enum TwoFactorSessionStatus
{
	Pending,
	Verified,
	Expired,
	Cancelled,
	Failed
}

