using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.TwoFactor.Nafath;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ACommerce.Authentication.AspNetCore.NafathWH.Controllers;

/// <summary>
/// Abstract base controller for Nafath webhook callbacks.
/// Inherit from this class in your API project to handle Nafath webhooks.
/// </summary>
/// <example>
/// <code>
/// [ApiController]
/// [Route("api/webhooks/nafath")]
/// public class NafathWebhookController : NafathWebhookControllerBase
/// {
///     public NafathWebhookController(
///         ITwoFactorAuthenticationProvider nafathProvider,
///         ILogger&lt;NafathWebhookController&gt; logger)
///         : base(nafathProvider, logger)
///     {
///     }
/// }
/// </code>
/// </example>
[ApiController]
public abstract class NafathWebhookControllerBase : ControllerBase
{
	protected readonly NafathAuthenticationProvider NafathProvider;
	protected readonly ILogger<NafathWebhookControllerBase> Logger;

	protected NafathWebhookControllerBase(
		ITwoFactorAuthenticationProvider nafathProvider,
		ILogger<NafathWebhookControllerBase> logger)
	{
		if (nafathProvider is not NafathAuthenticationProvider provider)
		{
			throw new ArgumentException(
				"Provider must be NafathAuthenticationProvider",
				nameof(nafathProvider));
		}

		NafathProvider = provider;
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Handles incoming Nafath webhook callbacks
	/// </summary>
	[HttpPost]
	public virtual async Task<IActionResult> HandleWebhook(
		[FromBody] NafathWebhookRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			Logger.LogInformation(
				"Received Nafath webhook - TransactionId: {TransactionId}, NationalId: {NationalId}, Status: {Status}",
				request.TransactionId,
				request.NationalId,
				request.Status);

			var success = await NafathProvider.HandleWebhookAsync(request, cancellationToken);

			if (!success)
			{
				Logger.LogWarning(
					"Nafath webhook processing failed for transaction: {TransactionId}",
					request.TransactionId);

				return Unauthorized(new
				{
					error = "WEBHOOK_VALIDATION_FAILED",
					message = "Invalid webhook request"
				});
			}

			Logger.LogInformation(
				"Nafath webhook processed successfully for transaction: {TransactionId}",
				request.TransactionId);

			return Ok(new
			{
				success = true,
				transactionId = request.TransactionId
			});
		}
		catch (Exception ex)
		{
			Logger.LogError(
				ex,
				"Error processing Nafath webhook for transaction: {TransactionId}",
				request.TransactionId);

			// Still return 200 to prevent webhook retries
			return Ok(new
			{
				success = false,
				error = "INTERNAL_ERROR"
			});
		}
	}

	/// <summary>
	/// Health check endpoint for Nafath webhook
	/// </summary>
	[HttpGet("health")]
	public virtual IActionResult Health()
	{
		return Ok(new
		{
			status = "healthy",
			provider = NafathProvider.ProviderName,
			timestamp = DateTimeOffset.UtcNow
		});
	}
}

