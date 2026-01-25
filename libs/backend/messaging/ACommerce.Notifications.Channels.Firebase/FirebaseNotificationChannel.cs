using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Channels.Firebase.Models;
using ACommerce.Notifications.Channels.Firebase.Options;
using ACommerce.Notifications.Channels.Firebase.Services;
using ACommerce.Notifications.Channels.Firebase.Storage;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Channels.Firebase;

/// <summary>
/// ???? ????? ????????? ??? Firebase Cloud Messaging
/// </summary>
public class FirebaseNotificationChannel : INotificationChannel
{
	private readonly FirebaseMessagingService _messagingService;
	private readonly IFirebaseTokenStore _tokenStore;
	private readonly FirebaseOptions _options;
	private readonly ILogger<FirebaseNotificationChannel> _logger;

	public NotificationChannel Channel => NotificationChannel.Firebase;

	public FirebaseNotificationChannel(
		FirebaseMessagingService messagingService,
		IFirebaseTokenStore tokenStore,
		FirebaseOptions options,
		ILogger<FirebaseNotificationChannel> logger)
	{
		_messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
		_tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<NotificationResult> SendAsync(
		Abstractions.Models.Notification notification,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation(
				"🚀 [Firebase] Starting to send notification {NotificationId} to user {UserId}",
				notification.Id,
				notification.UserId);

			// الحصول على Tokens المستخدم
			var deviceTokens = await _tokenStore.GetUserTokensAsync(
				notification.UserId,
				cancellationToken);

			_logger.LogInformation(
				"🔍 [Firebase] Found {TokenCount} tokens for user {UserId}",
				deviceTokens.Count,
				notification.UserId);

			if (!deviceTokens.Any())
			{
				_logger.LogWarning(
					"⚠️ [Firebase] No tokens found for user {UserId}",
					notification.UserId);

				return new NotificationResult
				{
					Success = false,
					NotificationId = notification.Id,
					ErrorMessage = "No Firebase tokens registered for user"
				};
			}

			// طباعة التوكنات (مخفية جزئياً)
			foreach (var token in deviceTokens)
			{
				var maskedToken = token.Token.Length > 20
					? $"{token.Token[..10]}...{token.Token[^10..]}"
					: token.Token;
				_logger.LogInformation(
					"📱 [Firebase] Token: {Token}, Platform: {Platform}, Active: {IsActive}",
					maskedToken, token.Platform, token.IsActive);
			}

			// بناء الرسالة
			var message = BuildMulticastMessage(notification, deviceTokens);

			_logger.LogInformation("📤 [Firebase] Sending message to Firebase...");

			// الإرسال
			var response = await _messagingService.SendMulticastAsync(
				deviceTokens.Select(t => t.Token),
				message,
				cancellationToken);

			_logger.LogInformation(
				"📊 [Firebase] Response: SuccessCount={Success}, FailureCount={Failure}",
				response.SuccessCount,
				response.FailureCount);

			// ?????? ???????
			await ProcessResponseAsync(response, deviceTokens, cancellationToken);

			var success = response.SuccessCount > 0;

			_logger.LogInformation(
				"Firebase notification {NotificationId} sent. Success: {Success}/{Total}",
				notification.Id,
				response.SuccessCount,
				deviceTokens.Count);

			return new NotificationResult
			{
				Success = success,
				NotificationId = notification.Id,
				ErrorMessage = success ? null : "Failed to send to all devices",
				Metadata = new Dictionary<string, object>
				{
					["totalDevices"] = deviceTokens.Count,
					["successCount"] = response.SuccessCount,
					["failureCount"] = response.FailureCount,
					["platforms"] = deviceTokens.GroupBy(t => t.Platform)
						.ToDictionary(g => g.Key.ToString(), g => g.Count())
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Failed to send Firebase notification {NotificationId}",
				notification.Id);

			return new NotificationResult
			{
				Success = false,
				NotificationId = notification.Id,
				ErrorMessage = ex.Message
			};
		}
	}

	public async Task<bool> ValidateAsync(
		Abstractions.Models.Notification notification,
		CancellationToken cancellationToken = default)
	{
		// ?????? ?? ???? Tokens
		var tokens = await _tokenStore.GetUserTokensAsync(
			notification.UserId,
			cancellationToken);

		if (!tokens.Any())
		{
			_logger.LogWarning(
				"Firebase validation failed for notification {NotificationId}: No tokens found",
				notification.Id);
			return false;
		}

		return true;
	}

	private MulticastMessage BuildMulticastMessage(
		Abstractions.Models.Notification notification,
		List<FirebaseDeviceToken> deviceTokens)
	{
		_logger.LogInformation(
			"Building Firebase message: Title={Title}, Body={Body}, Type={Type}, Tokens={TokenCount}",
			notification.Title,
			notification.Message,
			notification.Type,
			deviceTokens.Count);

		// بناء Data payload
		var data = new Dictionary<string, string>
		{
			["notificationId"] = notification.Id.ToString(),
			["type"] = notification.Type.ToString(),
			["priority"] = notification.Priority.ToString(),
			["createdAt"] = notification.CreatedAt.ToString("O")
		};

		// إضافة ActionUrl إذا وجد
		if (!string.IsNullOrEmpty(notification.ActionUrl))
		{
			data["actionUrl"] = notification.ActionUrl;
		}

		// إضافة البيانات الإضافية
		if (notification.Data != null)
		{
			foreach (var kvp in notification.Data)
			{
				data[kvp.Key] = kvp.Value?.ToString() ?? "";
			}
		}

		return new MulticastMessage
		{
			Notification = new FirebaseAdmin.Messaging.Notification
			{
				Title = notification.Title,
				Body = notification.Message
			},
			Data = data,
			Android = new AndroidConfig
			{
				Priority = Priority.High,
				Notification = new AndroidNotification
				{
					ChannelId = _options.DefaultChannelId,
					Sound = _options.DefaultSound,
					DefaultSound = true,
					NotificationPriority = NotificationPriority.PRIORITY_HIGH
				}
			},
			Apns = new ApnsConfig
			{
				Aps = new Aps
				{
					Sound = _options.DefaultSound,
					Badge = _options.EnableBadge ? 1 : 0,
					ContentAvailable = true
				}
			},
			Tokens = deviceTokens.Select(t => t.Token).ToList()
		};
	}

	private async Task ProcessResponseAsync(
		CustomBatchResponse response,
		List<FirebaseDeviceToken> deviceTokens,
		CancellationToken cancellationToken)
	{
		for (int i = 0; i < response.Responses.Count; i++)
		{
			var sendResponse = response.Responses[i];
			var deviceToken = deviceTokens[i];

			if (!sendResponse.IsSuccess)
			{
				var exception = sendResponse.Exception;

				_logger.LogWarning(
					"Failed to send to device {Platform} token {Token}: {Error}",
					deviceToken.Platform,
					MaskToken(deviceToken.Token),
					exception?.Message);

				// ??? ??? Token ??? ???? ?? ????? ????????
				if (exception is FirebaseMessagingException fex)
				{
					if (fex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
						fex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
					{
						_logger.LogInformation(
							"Deactivating invalid token for user {UserId}",
							deviceToken.UserId);

						await _tokenStore.DeactivateTokenAsync(
							deviceToken.Token,
							cancellationToken);
					}
				}
			}
		}
	}

	private static string MaskToken(string token)
	{
		if (string.IsNullOrEmpty(token) || token.Length < 10)
			return "***";

		return $"{token[..5]}...{token[^5..]}";
	}
}

