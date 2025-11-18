namespace ACommerce.Authentication.Abstractions;

/// <summary>
/// Defines the contract for two-factor authentication providers
/// </summary>
public interface ITwoFactorAuthenticationProvider
{
	/// <summary>
	/// Unique identifier for this provider (e.g., "Nafath", "SMS", "Email")
	/// </summary>
	string ProviderName { get; }

	/// <summary>
	/// Initiates a two-factor authentication flow
	/// </summary>
	Task<TwoFactorInitiationResult> InitiateAsync(
		TwoFactorInitiationRequest request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Verifies a two-factor authentication attempt
	/// </summary>
	Task<TwoFactorVerificationResult> VerifyAsync(
		TwoFactorVerificationRequest request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Cancels an ongoing two-factor authentication session
	/// </summary>
	Task<bool> CancelAsync(
		string transactionId,
		CancellationToken cancellationToken = default);
}

