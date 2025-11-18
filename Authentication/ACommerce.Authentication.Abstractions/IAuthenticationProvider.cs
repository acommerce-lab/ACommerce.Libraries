namespace ACommerce.Authentication.Abstractions;

/// <summary>
/// Defines the contract for authentication token providers (JWT, OpenIddict, etc.)
/// </summary>
public interface IAuthenticationProvider
{
	/// <summary>
	/// Unique identifier for this provider
	/// </summary>
	string ProviderName { get; }

	/// <summary>
	/// Authenticates a user and returns tokens
	/// </summary>
	Task<AuthenticationResult> AuthenticateAsync(
		AuthenticationRequest request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Refreshes an access token using a refresh token
	/// </summary>
	Task<AuthenticationResult> RefreshAsync(
		string refreshToken,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Validates a token
	/// </summary>
	Task<TokenValidationResult> ValidateTokenAsync(
		string token,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Revokes a token
	/// </summary>
	Task<bool> RevokeTokenAsync(
		string token,
		CancellationToken cancellationToken = default);
}

