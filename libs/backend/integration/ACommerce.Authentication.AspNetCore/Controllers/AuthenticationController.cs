using ACommerce.Authentication.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ACommerce.Authentication.AspNetCore.Controllers;

/// <summary>
/// Abstract base controller for authentication endpoints.
/// Inherit from this class in your API project to expose authentication endpoints.
/// </summary>
/// <example>
/// <code>
/// [ApiController]
/// [Route("api/auth")]
/// public class AuthController : AuthenticationControllerBase
/// {
///     public AuthController(
///         IAuthenticationProvider authProvider,
///         ITwoFactorAuthenticationProvider twoFactorProvider,
///         ILogger&lt;AuthController&gt; logger)
///         : base(authProvider, twoFactorProvider, logger)
///     {
///     }
/// }
/// </code>
/// </example>
[ApiController]
public abstract class AuthenticationControllerBase : ControllerBase
{
	protected readonly IAuthenticationProvider AuthProvider;
	protected readonly ITwoFactorAuthenticationProvider TwoFactorProvider;
	protected readonly ILogger<AuthenticationControllerBase> Logger;

	protected AuthenticationControllerBase(
		IAuthenticationProvider authProvider,
		ITwoFactorAuthenticationProvider twoFactorProvider,
		ILogger<AuthenticationControllerBase> logger)
	{
		AuthProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
		TwoFactorProvider = twoFactorProvider ?? throw new ArgumentNullException(nameof(twoFactorProvider));
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Initiates two-factor authentication
	/// </summary>
	[HttpPost("2fa/initiate")]
	public virtual async Task<IActionResult> InitiateTwoFactor(
		[FromBody] TwoFactorInitiationRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			Logger.LogInformation(
				"Initiating 2FA for identifier: {Identifier} with provider: {Provider}",
				request.Identifier,
				TwoFactorProvider.ProviderName);

			var result = await TwoFactorProvider.InitiateAsync(request, cancellationToken);

			if (!result.Success)
			{
				Logger.LogWarning(
					"2FA initiation failed: {Error}",
					result.Error?.Code);

				return BadRequest(new
				{
					error = result.Error?.Code,
					message = result.Error?.Message,
					details = result.Error?.Details
				});
			}

			return Ok(new
			{
				success = true,
				transactionId = result.TransactionId,
				verificationCode = result.VerificationCode,
				message = result.Message,
				expiresIn = result.ExpiresIn?.TotalSeconds,
				provider = TwoFactorProvider.ProviderName
			});
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Unexpected error during 2FA initiation");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// Verifies two-factor authentication and generates access token
	/// </summary>
	[HttpPost("2fa/verify")]
	public virtual async Task<IActionResult> VerifyTwoFactor(
		[FromBody] TwoFactorVerificationRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			Logger.LogInformation(
				"Verifying 2FA for transaction: {TransactionId}",
				request.TransactionId);

			// Step 1: Verify 2FA
			var verifyResult = await TwoFactorProvider.VerifyAsync(request, cancellationToken);

			if (!verifyResult.Success)
			{
				Logger.LogWarning(
					"2FA verification failed: {Error}",
					verifyResult.Error?.Code);

				return BadRequest(new
				{
					error = verifyResult.Error?.Code,
					message = verifyResult.Error?.Message
				});
			}

			// Step 2: Generate authentication token
			var tokenResult = await AuthProvider.AuthenticateAsync(
				new AuthenticationRequest
				{
					Identifier = verifyResult.UserId!,
					Claims = verifyResult.UserClaims?.ToDictionary(k => k.Key, v => v.Value),
					Metadata = new Dictionary<string, object>
					{
						["two_factor_provider"] = TwoFactorProvider.ProviderName,
						["two_factor_verified"] = true,
						["verification_timestamp"] = DateTimeOffset.UtcNow
					}
				},
				cancellationToken);

			if (!tokenResult.Success)
			{
				Logger.LogError(
					"Token generation failed after successful 2FA: {Error}",
					tokenResult.Error?.Code);

				return StatusCode(500, new
				{
					error = tokenResult.Error?.Code,
					message = tokenResult.Error?.Message
				});
			}

			Logger.LogInformation(
				"2FA verification and token generation successful for user: {UserId}",
				tokenResult.UserId);

			return Ok(new
			{
				success = true,
				accessToken = tokenResult.AccessToken,
				refreshToken = tokenResult.RefreshToken,
				tokenType = tokenResult.TokenType,
				expiresAt = tokenResult.ExpiresAt,
				userId = tokenResult.UserId
			});
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Unexpected error during 2FA verification");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// Cancels an ongoing two-factor authentication session
	/// </summary>
	[HttpPost("2fa/cancel")]
	public virtual async Task<IActionResult> CancelTwoFactor(
		[FromBody] string transactionId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			Logger.LogInformation(
				"Cancelling 2FA session: {TransactionId}",
				transactionId);

			var result = await TwoFactorProvider.CancelAsync(transactionId, cancellationToken);

			return Ok(new
			{
				success = result,
				transactionId
			});
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Error cancelling 2FA session");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "Failed to cancel session"
			});
		}
	}

	/// <summary>
	/// Refreshes an access token using a refresh token
	/// </summary>
	[HttpPost("refresh")]
	public virtual async Task<IActionResult> RefreshToken(
		[FromBody] string refreshToken,
		CancellationToken cancellationToken = default)
	{
		try
		{
			Logger.LogInformation("Token refresh requested");

			var result = await AuthProvider.RefreshAsync(refreshToken, cancellationToken);

			if (!result.Success)
			{
				Logger.LogWarning("Token refresh failed: {Error}", result.Error?.Code);

				return BadRequest(new
				{
					error = result.Error?.Code,
					message = result.Error?.Message
				});
			}

			return Ok(new
			{
				success = true,
				accessToken = result.AccessToken,
				refreshToken = result.RefreshToken,
				tokenType = result.TokenType,
				expiresAt = result.ExpiresAt
			});
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Unexpected error during token refresh");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// Gets information about the current authentication configuration
	/// </summary>
	[HttpGet("info")]
	public virtual IActionResult GetInfo()
	{
		return Ok(new
		{
			authenticationProvider = AuthProvider.ProviderName,
			twoFactorProvider = TwoFactorProvider.ProviderName
		});
	}
}

