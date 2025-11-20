using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.TwoFactor.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.Authentication.TwoFactor.SMS;

public class SmsAuthenticationProvider : ITwoFactorAuthenticationProvider
{
	private readonly ISmsService _smsService;
	private readonly ITwoFactorSessionStore _sessionStore;
	private readonly SmsOptions _options;
	private readonly ILogger<SmsAuthenticationProvider> _logger;
	private static readonly Random _random = new();

	public string ProviderName => "SMS";

	public SmsAuthenticationProvider(
		ISmsService smsService,
		ITwoFactorSessionStore sessionStore,
		IOptions<SmsOptions> options,
		ILogger<SmsAuthenticationProvider> logger)
	{
		_smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
		_sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
		_options = options.Value ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<TwoFactorInitiationResult> InitiateAsync(
		TwoFactorInitiationRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var code = GenerateVerificationCode();
			var transactionId = Guid.NewGuid().ToString();
			var expiresIn = _options.CodeLifetime;

			var session = new TwoFactorSession
			{
				TransactionId = transactionId,
				Identifier = request.Identifier,
				Provider = ProviderName,
				CreatedAt = DateTimeOffset.UtcNow,
				ExpiresAt = DateTimeOffset.UtcNow.Add(expiresIn),
				VerificationCode = code,
				Status = TwoFactorSessionStatus.Pending,
				Metadata = request.Metadata
			};

			await _sessionStore.CreateSessionAsync(session, cancellationToken);

			var message = string.Format(_options.MessageTemplate, code);
			await _smsService.SendAsync(request.Identifier, message, cancellationToken);

			_logger.LogInformation(
				"SMS verification code sent to {PhoneNumber}",
				MaskPhoneNumber(request.Identifier));

			return new TwoFactorInitiationResult
			{
				Success = true,
				TransactionId = transactionId,
				Message = "?? ????? ??? ?????? ??? ??? ?????",
				ExpiresIn = expiresIn
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send SMS verification code");

			return new TwoFactorInitiationResult
			{
				Success = false,
				Error = new TwoFactorError
				{
					Code = "SMS_SEND_FAILED",
					Message = "??? ????? ??? ??????"
				}
			};
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
				return CreateErrorResult("SESSION_NOT_FOUND", "?????? ??? ??????");
			}

			if (session.ExpiresAt < DateTimeOffset.UtcNow)
			{
				await UpdateSessionStatusAsync(
					session.TransactionId,
					TwoFactorSessionStatus.Expired,
					cancellationToken);

				return CreateErrorResult("CODE_EXPIRED", "????? ?????? ?????");
			}

			if (session.VerificationCode != request.VerificationCode)
			{
				_logger.LogWarning(
					"Invalid verification code attempt for session {TransactionId}",
					request.TransactionId);

				return CreateErrorResult("INVALID_CODE", "????? ??? ????");
			}

			await UpdateSessionStatusAsync(
				session.TransactionId,
				TwoFactorSessionStatus.Verified,
				cancellationToken);

			return new TwoFactorVerificationResult
			{
				Success = true,
				UserId = session.Identifier,
				UserClaims = new Dictionary<string, string>
				{
					["phone_number"] = session.Identifier,
					["provider"] = ProviderName
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during SMS verification");
			return CreateErrorResult("VERIFICATION_ERROR", "??? ??? ????? ??????");
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

			return true;
		}
		catch
		{
			return false;
		}
	}

	private string GenerateVerificationCode()
	{
		return _random.Next(100000, 999999).ToString();
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

	private TwoFactorVerificationResult CreateErrorResult(string code, string message)
	{
		return new TwoFactorVerificationResult
		{
			Success = false,
			Error = new TwoFactorError
			{
				Code = code,
				Message = message
			}
		};
	}

	private static string MaskPhoneNumber(string phoneNumber)
	{
		if (phoneNumber.Length < 4) return "****";
		return $"****{phoneNumber[^4..]}";
	}
}

/// <summary>
/// Interface for SMS sending service
/// Implement this interface with your preferred SMS provider (Twilio, AWS SNS, etc.)
/// </summary>
public interface ISmsService
{
	Task SendAsync(
		string phoneNumber,
		string message,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// Fake SMS service for development/testing
/// </summary>
public class FakeSmsService : ISmsService
{
	private readonly ILogger<FakeSmsService> _logger;

	public FakeSmsService(ILogger<FakeSmsService> logger)
	{
		_logger = logger;
	}

	public Task SendAsync(
		string phoneNumber,
		string message,
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation(
			"?? [FAKE SMS] To: {PhoneNumber} | Message: {Message}",
			phoneNumber,
			message);

		return Task.CompletedTask;
	}
}

public class SmsOptions
{
	public const string SectionName = "Authentication:TwoFactor:SMS";

	public TimeSpan CodeLifetime { get; set; } = TimeSpan.FromMinutes(5);

	public string MessageTemplate { get; set; } = "??? ?????? ????? ?? ??: {0}";
}

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSmsTwoFactor(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddOptions<SmsOptions>()
			.Bind(configuration.GetSection(SmsOptions.SectionName))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.AddScoped<ITwoFactorAuthenticationProvider, SmsAuthenticationProvider>();

		return services;
	}

	public static IServiceCollection AddSmsTwoFactor(
		this IServiceCollection services,
		Action<SmsOptions> configure)
	{
		services.AddOptions<SmsOptions>()
			.Configure(configure)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.AddScoped<ITwoFactorAuthenticationProvider, SmsAuthenticationProvider>();

		return services;
	}

	/// <summary>
	/// Adds fake SMS service for development/testing
	/// </summary>
	public static IServiceCollection AddFakeSmsService(this IServiceCollection services)
	{
		services.AddScoped<ISmsService, FakeSmsService>();
		return services;
	}
}

