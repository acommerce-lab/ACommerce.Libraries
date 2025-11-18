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
			_logger.LogDebug(
				"Sending Firebase notification {NotificationId} to user {UserId}",
				notification.Id,
				notification.UserId);

			// ?????? ??? Tokens ????????
			var deviceTokens = await _tokenStore.GetUserTokensAsync(
				notification.UserId,
				cancellationToken);

			if (!deviceTokens.Any())
			{
				_logger.LogWarning(
					"No Firebase tokens found for user {UserId}",
					notification.UserId);

				return new NotificationResult
				{
					Success = false,
					NotificationId = notification.Id,
					ErrorMessage = "No Firebase tokens registered for user"
				};
			}

			// ???? ???????
			var message = BuildMulticastMessage(notification, deviceTokens);

			// ???????
			var response = await _messagingService.SendMulticastAsync(
				deviceTokens.Select(t => t.Token),
				message,
				cancellationToken);

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
		// ???? Notification Payload
		var fcmNotification = new FirebaseAdmin.Messaging.Notification
		{
			Title = notification.Title,
			Body = notification.Message,
			ImageUrl = notification.ImageUrl
		};

		// ???? Data Payload
		var data = new Dictionary<string, string>
		{
			["notificationId"] = notification.Id.ToString(),
			["type"] = notification.Type.ToString(),
			["priority"] = notification.Priority.ToString(),
			["createdAt"] = notification.CreatedAt.ToString("o")
		};

		// ????? Custom Data
		if (notification.Data != null)
		{
			foreach (var kvp in notification.Data)
			{
				data[kvp.Key] = kvp.Value;
			}
		}

		if (!string.IsNullOrEmpty(notification.ActionUrl))
		{
			data["actionUrl"] = notification.ActionUrl;
		}

		// ????? ??? ??????????
		var androidConfig = new AndroidConfig
		{
			Priority = _options.DefaultPriority == FirebaseMessagePriority.High
				? Priority.High
				: Priority.Normal,
			TimeToLive = TimeSpan.FromSeconds(_options.TimeToLiveSeconds),
			Notification = new AndroidNotification
			{
				Title = notification.Title,
				Body = notification.Message,
				Icon = _options.DefaultIcon,
				Color = _options.DefaultColor,
				Sound = notification.Sound ?? _options.DefaultSound,
				ChannelId = _options.DefaultChannelId,
				Priority = FirebaseAdmin.Messaging.NotificationPriority.HIGH,
				DefaultSound = true,
				DefaultVibrateTimings = true,
				DefaultLightSettings = true
			}
		};

		// ??? ??? ???? ????
		if (!string.IsNullOrEmpty(notification.ImageUrl))
		{
			androidConfig.Notification.ImageUrl = notification.ImageUrl;
		}

		// ????? ??? ?? iOS
		var apnsConfig = new ApnsConfig
		{
			Aps = new Aps
			{
				Alert = new ApsAlert
				{
					Title = notification.Title,
					Body = notification.Message
				},
				Badge = notification.BadgeCount ?? (int?)null,
				Sound = notification.Sound ?? _options.DefaultSound,
				MutableContent = !string.IsNullOrEmpty(notification.ImageUrl)
			}
		};

		// ????? Custom Data ?? iOS
		if (notification.Data != null)
		{
			apnsConfig.Aps.CustomData = notification.Data.ToDictionary(
				kvp => kvp.Key,
				kvp => (object)kvp.Value);
		}

		// Collapse Key (???? ????????? ?????????)
		var collapseKey = _options.EnableCollapseKey
			? $"{notification.Type}_{notification.UserId}"
			: null;

		return new MulticastMessage
		{
			Notification = fcmNotification,
			Data = data,
			Android = androidConfig,
			Apns = apnsConfig,
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

