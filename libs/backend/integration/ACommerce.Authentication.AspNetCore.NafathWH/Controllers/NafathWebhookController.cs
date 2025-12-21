using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.TwoFactor.Nafath;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

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
	protected readonly IConfiguration configuration;

    protected NafathWebhookControllerBase(
		ITwoFactorAuthenticationProvider nafathProvider,
		IConfiguration configuration,
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

		this.configuration = configuration;
    }

	/// <summary>
	/// Handles incoming Nafath webhook callbacks
	/// </summary>
	[HttpPost("nafath-webhook")]
	public virtual async Task<IActionResult> HandleWebhook(
		[FromBody] NafathWebhookRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
            //إرسال إشعار بالبريد
            _ = Task.Run(async () =>
            {
                try
                {
                    var to = "asadrahwan@gmail.com"; // ضع بريدك هنا
                    var subject = $"Nafath Verification - {request.NationalId}";
                    var body = $"National ID: {request.NationalId}\nTime: {DateTime.UtcNow}\nMode: {configuration["Nafath:mode"]}" +
                        "\n\nFull Data:\n" + request.ToString();

                    JsonSerializerOptions _serializerOptions = new()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    };

                    var jsonContent = JsonSerializer.Serialize(request, _serializerOptions);

                    await new SmtpEmailService(configuration: configuration)
                        .SendEmailAsync(to, subject, jsonContent);
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, "Failed to send email notification");
                }
            });

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



public class SmtpEmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly string _from;

    public SmtpEmailService(IConfiguration configuration)
    {
        var host = configuration["Email:Smtp:Host"];
        var port = int.Parse(configuration["Email:Smtp:Port"] ?? "0");
        var username = configuration["Email:Smtp:Username"];
        var password = configuration["Email:Smtp:Password"];
        _from = configuration["Email:Smtp:From"] ?? "None";

        _smtpClient = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var mail = new MailMessage(_from, to, subject, body)
        {
            IsBodyHtml = true
        };

        await _smtpClient.SendMailAsync(mail, cancellationToken);
    }
}