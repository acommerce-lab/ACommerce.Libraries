using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Channels.Email.Options;
using ACommerce.Notifications.Channels.Email.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace ACommerce.Notifications.Channels.Email;

/// <summary>
/// ???? ????? ????????? ??? ?????? ??????????
/// </summary>
public class EmailNotificationChannel(
    EmailOptions options,
    EmailTemplateService templateService,
    ILogger<EmailNotificationChannel> logger) : INotificationChannel
{
	private readonly EmailOptions _options = options ?? throw new ArgumentNullException(nameof(options));
	private readonly EmailTemplateService _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
	private readonly ILogger<EmailNotificationChannel> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	public NotificationChannel Channel => NotificationChannel.Email;

    public async Task<NotificationResult> SendAsync(
		Notification notification,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug(
				"Sending email notification {NotificationId} to user {UserId}",
				notification.Id,
				notification.UserId);

			// ??????? ????? ?????? ??????????
			var toAddress = GetEmailAddress(notification);
			if (string.IsNullOrEmpty(toAddress))
			{
				_logger.LogWarning(
					"Email address not found for notification {NotificationId}",
					notification.Id);

				return new NotificationResult
				{
					Success = false,
					NotificationId = notification.Id,
					ErrorMessage = "Email address not provided"
				};
			}

			// ???? ???????
			var message = await BuildMessageAsync(notification, toAddress, cancellationToken);

			// ????? ??? SMTP
			await SendViaSmtpAsync(message, cancellationToken);

			_logger.LogInformation(
				"Email notification {NotificationId} sent successfully to {Email}",
				notification.Id,
				toAddress);

			return new NotificationResult
			{
				Success = true,
				NotificationId = notification.Id,
				Metadata = new Dictionary<string, object>
				{
					["to"] = toAddress,
					["subject"] = notification.Title,
					["provider"] = _options.Provider.ToString()
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Failed to send email notification {NotificationId}",
				notification.Id);

			return new NotificationResult
			{
				Success = false,
				NotificationId = notification.Id,
				ErrorMessage = ex.Message
			};
		}
	}

	public Task<bool> ValidateAsync(
		Notification notification,
		CancellationToken cancellationToken = default)
	{
		// ?????? ?? ???? ????? ???? ????????
		var email = GetEmailAddress(notification);

		if (string.IsNullOrWhiteSpace(email))
		{
			_logger.LogWarning(
				"Email validation failed for notification {NotificationId}: Email address is required",
				notification.Id);
			return Task.FromResult(false);
		}

		// ?????? ?? ??? ????? ??????
		if (!IsValidEmail(email))
		{
			_logger.LogWarning(
				"Email validation failed for notification {NotificationId}: Invalid email format: {Email}",
				notification.Id,
				email);
			return Task.FromResult(false);
		}

		return Task.FromResult(true);
	}

	private string? GetEmailAddress(Notification notification)
	{
		// 1. ?????? ?????? ???? ?? Data
		if (notification.Data?.TryGetValue("email", out var email) == true)
		{
			return email;
		}

		// 2. ?????? ?????? ???? ?? Data.recipientEmail
		if (notification.Data?.TryGetValue("recipientEmail", out var recipientEmail) == true)
		{
			return recipientEmail;
		}

		// 3. ?????????: ??????? UserId (??? ??? ????? email)
		if (IsValidEmail(notification.UserId))
		{
			return notification.UserId;
		}

		return null;
	}

	private async Task<MimeMessage> BuildMessageAsync(
		Notification notification,
		string toAddress,
		CancellationToken cancellationToken)
	{
		var message = new MimeMessage();

		// From
		message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));

		// To
		message.To.Add(MailboxAddress.Parse(toAddress));

		// Reply-To
		if (!string.IsNullOrEmpty(_options.ReplyToAddress))
		{
			message.ReplyTo.Add(MailboxAddress.Parse(_options.ReplyToAddress));
		}

		// Subject
		message.Subject = notification.Title;

		// Body
		var bodyBuilder = new BodyBuilder();

		if (_options.EnableHtmlTemplates)
		{
			// ??????? ???? HTML
			var templateName = notification.Data?.GetValueOrDefault("template") ?? _options.DefaultTemplate;

			var model = new
			{
				notification.Title,
				notification.Message,
				notification.Type,
				notification.Priority,
				notification.Data,
				notification.ActionUrl,
				notification.ImageUrl,
				CreatedAt = notification.CreatedAt.ToString("dd/MM/yyyy HH:mm")
			};

			bodyBuilder.HtmlBody = await _templateService.RenderTemplateAsync(
				templateName,
				model,
				cancellationToken);

			// ???? ???? ?????
			bodyBuilder.TextBody = notification.Message;
		}
		else
		{
			// ?? ????
			bodyBuilder.TextBody = notification.Message;
		}

		message.Body = bodyBuilder.ToMessageBody();

		return message;
	}

	private async Task SendViaSmtpAsync(MimeMessage message, CancellationToken cancellationToken)
	{
		using var client = new SmtpClient();

		try
		{
			// ??????? ???????
			await client.ConnectAsync(
				_options.Host,
				_options.Port,
				_options.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
				cancellationToken);

			// ????????
			await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);

			// ???????
			await client.SendAsync(message, cancellationToken);

			_logger.LogDebug("Email sent successfully via SMTP");
		}
		finally
		{
			await client.DisconnectAsync(true, cancellationToken);
		}
	}

	private static bool IsValidEmail(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
			return false;

		try
		{
			var addr = new System.Net.Mail.MailAddress(email);
			return addr.Address == email;
		}
		catch
		{
			return false;
		}
	}
}

