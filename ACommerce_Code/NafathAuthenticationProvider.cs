using System.Net.Http.Json;
using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.TwoFactor.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.Authentication.TwoFactor.Nafath;

public class NafathAuthenticationProvider : ITwoFactorAuthenticationProvider
{
	private readonly HttpClient _httpClient;
	private readonly NafathOptions _options;
	private readonly ITwoFactorSessionStore _sessionStore;
	private readonly ILogger<NafathAuthenticationProvider> _logger;

	public string ProviderName => "Nafath";

	public NafathAuthenticationProvider(
		IHttpClientFactory httpClientFactory,
		IOptions<NafathOptions> options,
		ITwoFactorSessionStore sessionStore,
		ILogger<NafathAuthenticationProvider> logger)
	{
		_httpClient = httpClientFactory.CreateClient(NafathOptions.HttpClientName);
		_options = options.Value ?? throw new ArgumentNullException(nameof(options));
		_sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));

		ValidateOptions();
		ConfigureHttpClient();
	}

	public async Task<TwoFactorInitiationResult> InitiateAsync(
		TwoFactorInitiationRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			// Test mode
			if (_options.Mode == NafathMode.Test &&
				request.Identifier == _options.TestNationalId)
			{
				return await HandleTestModeInitiationAsync(request, cancellationToken);
			}

			// Production mode
			var payload = new { national_id = request.Identifier };

			var httpRequest = new HttpRequestMessage(HttpMethod.Post, "verify-by-nafath")
			{
				Content = JsonContent.Create(payload)
			};

			httpRequest.Headers.Add("X-Authorization", _options.ApiKey);

			var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
				_logger.LogWarning(
					"Nafath initiation failed: {StatusCode}, Content: {Content}",
					response.StatusCode,
					errorContent);

				return new TwoFactorInitiationResult
				{
					Success = false,
					Error = new TwoFactorError
					{
						Code = "NAFATH_API_ERROR",
						Message = "??? ??????? ????? ????",
						Details = $"HTTP {response.StatusCode}"
					}
				};
			}

			var apiResponse = await response.Content
				.ReadFromJsonAsync<NafathApiInitResponse>(cancellationToken);

			if (apiResponse == null)
			{
				return CreateInitiationError("NAFATH_INVALID_RESPONSE", "??????? ??? ????? ?? ????");
			}

			// Store session
			var expiresIn = TimeSpan.FromMinutes(_options.SessionExpirationMinutes);
			var session = new TwoFactorSession
			{
				TransactionId = apiResponse.TransactionId,
				Identifier = request.Identifier,
				Provider = ProviderName,
				CreatedAt = DateTimeOffset.UtcNow,
				ExpiresAt = DateTimeOffset.UtcNow.Add(expiresIn),
				VerificationCode = apiResponse.Code,
				Status = TwoFactorSessionStatus.Pending,
				Metadata = request.Metadata
			};

			await _sessionStore.CreateSessionAsync(session, cancellationToken);

			_logger.LogInformation(
				"Nafath session initiated successfully. TransactionId: {TransactionId}, NationalId: {NationalId}",
				apiResponse.TransactionId,
				request.Identifier);

			return new TwoFactorInitiationResult
			{
				Success = true,
				TransactionId = apiResponse.TransactionId,
				VerificationCode = apiResponse.Code,
				Message = apiResponse.Message ?? "???? ?????? ?? ???? ????? ????",
				ExpiresIn = expiresIn
			};
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "HTTP error during Nafath initiation");
			return CreateInitiationError("NAFATH_CONNECTION_ERROR", "??? ??????? ????? ????");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error during Nafath initiation");
			return CreateInitiationError("NAFATH_INITIATION_ERROR", "??? ??? ??? ?????");
		}
	}

	public async Task<TwoFactorVerificationResult> VerifyAsync(
		TwoFactorVerificationRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var session = await _sessionStore.GetSessionAsync(
				request.TransactionId,
				cancellationToken);

			if (session == null)
			{
				return CreateVerificationError("SESSION_NOT_FOUND", "?????? ??? ??????");
			}

			if (session.Provider != ProviderName)
			{
				return CreateVerificationError("INVALID_PROVIDER", "???? ???????? ??? ????");
			}

			if (session.Status == TwoFactorSessionStatus.Verified)
			{
				// Already verified
				return new TwoFactorVerificationResult
				{
					Success = true,
					UserId = session.Identifier,
					UserClaims = BuildUserClaims(session)
				};
			}

			if (session.Status != TwoFactorSessionStatus.Pending)
			{
				return CreateVerificationError(
					"SESSION_INVALID_STATUS",
					$"???? ?????? ??? ?????: {session.Status}");
			}

			if (session.ExpiresAt < DateTimeOffset.UtcNow)
			{
				await UpdateSessionStatusAsync(
					session.TransactionId,
					TwoFactorSessionStatus.Expired,
					cancellationToken);

				return CreateVerificationError("SESSION_EXPIRED", "????? ?????? ??????");
			}

			// In production, verification happens via webhook
			// This endpoint just checks the current status
			return new TwoFactorVerificationResult
			{
				Success = false,
				Error = new TwoFactorError
				{
					Code = "VERIFICATION_PENDING",
					Message = "?? ?????? ?????? ??? ????? ????"
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during Nafath verification check");
			return CreateVerificationError("VERIFICATION_ERROR", "??? ??? ????? ??????");
		}
	}

	public async Task<bool> CancelAsync(
		string transactionId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await UpdateSessionStatusAsync(
				transactionId,
				TwoFactorSessionStatus.Cancelled,
				cancellationToken);

			_logger.LogInformation("Nafath session cancelled: {TransactionId}", transactionId);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error cancelling Nafath session: {TransactionId}", transactionId);
			return false;
		}
	}

	/// <summary>
	/// Handles Nafath webhook callback (should be called from webhook endpoint)
	/// </summary>
	public async Task<bool> HandleWebhookAsync(
		NafathWebhookRequest webhookRequest,
		CancellationToken cancellationToken = default)
	{
		try
		{
			// Verify webhook secret
			if (webhookRequest.Password != _options.WebhookSecret)
			{
				_logger.LogWarning(
					"Invalid webhook password for transaction {TransactionId}",
					webhookRequest.TransactionId);
				return false;
			}

			var session = await _sessionStore.GetSessionAsync(
				webhookRequest.TransactionId,
				cancellationToken);

			if (session == null)
			{
				_logger.LogWarning(
					"Webhook received for unknown session: {TransactionId}",
					webhookRequest.TransactionId);
				return false;
			}

			// Check if status is COMPLETED
			if (webhookRequest.Status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase))
			{
				await UpdateSessionStatusAsync(
					session.TransactionId,
					TwoFactorSessionStatus.Verified,
					cancellationToken);

				_logger.LogInformation(
					"Nafath verification completed successfully for {NationalId}",
					webhookRequest.NationalId);

				return true;
			}
			else
			{
				await UpdateSessionStatusAsync(
					session.TransactionId,
					TwoFactorSessionStatus.Failed,
					cancellationToken);

				_logger.LogWarning(
					"Nafath verification failed for {NationalId}. Status: {Status}",
					webhookRequest.NationalId,
					webhookRequest.Status);

				return false;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error handling Nafath webhook");
			return false;
		}
	}

	private async Task<TwoFactorInitiationResult> HandleTestModeInitiationAsync(
		TwoFactorInitiationRequest request,
		CancellationToken cancellationToken)
	{
		var transactionId = Guid.NewGuid().ToString();
		var expiresIn = TimeSpan.FromMinutes(_options.SessionExpirationMinutes);

		var session = new TwoFactorSession
		{
			TransactionId = transactionId,
			Identifier = request.Identifier,
			Provider = ProviderName,
			CreatedAt = DateTimeOffset.UtcNow,
			ExpiresAt = DateTimeOffset.UtcNow.Add(expiresIn),
			VerificationCode = "00",
			Status = TwoFactorSessionStatus.Verified, // Auto-verify in test mode
			Metadata = request.Metadata
		};

		await _sessionStore.CreateSessionAsync(session, cancellationToken);

		_logger.LogInformation(
			"Test mode: Nafath session auto-verified for {Identifier}",
			request.Identifier);

		return new TwoFactorInitiationResult
		{
			Success = true,
			TransactionId = transactionId,
			VerificationCode = "00",
			Message = "??? ????????: ?? ?????? ????????",
			ExpiresIn = expiresIn
		};
	}

	private async Task UpdateSessionStatusAsync(
		string transactionId,
		TwoFactorSessionStatus status,
		CancellationToken cancellationToken)
	{
		var session = await _sessionStore.GetSessionAsync(transactionId, cancellationToken);
		if (session != null)
		{
			var updatedSession = session with { Status = status };
			await _sessionStore.UpdateSessionAsync(updatedSession, cancellationToken);
		}
	}

	private Dictionary<string, string> BuildUserClaims(TwoFactorSession session)
	{
		var claims = new Dictionary<string, string>
		{
			["national_id"] = session.Identifier,
			["provider"] = ProviderName,
			["verification_method"] = "nafath"
		};

		if (session.Metadata != null)
		{
			foreach (var meta in session.Metadata)
			{
				claims[$"meta_{meta.Key}"] = meta.Value;
			}
		}

		return claims;
	}

	private void ConfigureHttpClient()
	{
		_httpClient.DefaultRequestHeaders.Clear();
		_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
	}

	private void ValidateOptions()
	{
		if (string.IsNullOrWhiteSpace(_options.BaseUrl))
			throw new InvalidOperationException("Nafath BaseUrl is required");

		if (_options.Mode == NafathMode.Production)
		{
			if (string.IsNullOrWhiteSpace(_options.ApiKey))
				throw new InvalidOperationException("Nafath ApiKey is required in production mode");

			if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
				throw new InvalidOperationException("Nafath WebhookSecret is required in production mode");
		}
	}

	private TwoFactorInitiationResult CreateInitiationError(string code, string message)
	{
		return new TwoFactorInitiationResult
		{
			Success = false,
			Error = new TwoFactorError { Code = code, Message = message }
		};
	}

	private TwoFactorVerificationResult CreateVerificationError(string code, string message)
	{
		return new TwoFactorVerificationResult
		{
			Success = false,
			Error = new TwoFactorError { Code = code, Message = message }
		};
	}
}

// DTOs for Nafath API
internal record NafathApiInitResponse
{
	public string TransactionId { get; init; } = default!;
	public string Code { get; init; } = default!;
	public string? Message { get; init; }
}

public record NafathWebhookRequest
{
	public string TransactionId { get; init; } = default!;
	public string NationalId { get; init; } = default!;
	public string Status { get; init; } = default!;
	public string Password { get; init; } = default!;
}

