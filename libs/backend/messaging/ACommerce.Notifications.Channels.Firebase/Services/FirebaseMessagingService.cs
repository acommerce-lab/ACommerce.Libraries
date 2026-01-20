using ACommerce.Notifications.Channels.Firebase.Models;
using ACommerce.Notifications.Channels.Firebase.Options;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Channels.Firebase.Services;

/// <summary>
/// ???? ????? Firebase Cloud Messaging
/// </summary>
public class FirebaseMessagingService
{
	private readonly FirebaseOptions _options;
	private readonly ILogger<FirebaseMessagingService> _logger;
	private FirebaseMessaging? _messaging;
	private bool _isInitialized = false;
	private readonly object _initLock = new();

	public FirebaseMessagingService(
		FirebaseOptions options,
		ILogger<FirebaseMessagingService> logger)
	{
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// ????? Firebase Admin SDK
	/// </summary>
	private void EnsureInitialized()
	{
		if (_isInitialized) return;

		lock (_initLock)
		{
			if (_isInitialized) return;

			try
			{
				GoogleCredential credential;

				// ??????? 1: ?? ??? JSON
				if (!string.IsNullOrEmpty(_options.ServiceAccountKeyPath))
				{
					if (!File.Exists(_options.ServiceAccountKeyPath))
					{
						throw new FileNotFoundException(
							$"Firebase service account key not found: {_options.ServiceAccountKeyPath}");
					}

					credential = GoogleCredential.FromFile(_options.ServiceAccountKeyPath);

					_logger.LogInformation(
						"Firebase initialized from file: {Path}",
						_options.ServiceAccountKeyPath);
				}
				// ??????? 2: ?? JSON ?????? (Environment Variable)
				else if (!string.IsNullOrEmpty(_options.ServiceAccountKeyJson))
				{
					credential = GoogleCredential.FromJson(_options.ServiceAccountKeyJson);

					_logger.LogInformation("Firebase initialized from JSON string");
				}
				else
				{
					throw new InvalidOperationException(
						"Either ServiceAccountKeyPath or ServiceAccountKeyJson must be provided");
				}

				// ????? Firebase App
				var app = FirebaseApp.Create(new AppOptions
				{
					Credential = credential,
					ProjectId = _options.ProjectId
				});

				_messaging = FirebaseMessaging.GetMessaging(app);
				_isInitialized = true;

				_logger.LogInformation("Firebase Cloud Messaging initialized successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to initialize Firebase");
				throw;
			}
		}
	}

	/// <summary>
	/// ????? ????? ??? ???? ????
	/// </summary>
	public async Task<string> SendAsync(
		string token,
		Message message,
		CancellationToken cancellationToken = default)
	{
		EnsureInitialized();

		try
		{
			_logger.LogDebug("Sending FCM message to token: {Token}", MaskToken(token));

			var response = await _messaging!.SendAsync(message, _options.DryRun, cancellationToken);

			_logger.LogInformation(
				"FCM message sent successfully. Message ID: {MessageId}",
				response);

			return response;
		}
		catch (FirebaseMessagingException ex)
		{
			_logger.LogError(
				ex,
				"Firebase messaging error: {ErrorCode} - {Message}",
				ex.MessagingErrorCode,
				ex.Message);

			throw;
		}
	}

	/// <summary>
	/// ????? ????? ??? ??? ????? (Batch)
	/// </summary>
	public async Task<CustomBatchResponse> SendMulticastAsync(
		IEnumerable<string> tokens,
		MulticastMessage message,
		CancellationToken cancellationToken = default)
	{
		EnsureInitialized();

		var tokenList = tokens.ToList();

		if (tokenList.Count == 0)
		{
			_logger.LogWarning("No tokens provided for multicast");
			return new CustomBatchResponse
			{
				SuccessCount = 0,
				FailureCount = 0,
				Responses = []
			};
		}

		if (tokenList.Count > _options.MaxBatchSize)
		{
			_logger.LogWarning(
				"Token count {Count} exceeds max batch size {MaxSize}. Sending in batches.",
				tokenList.Count,
				_options.MaxBatchSize);

			// ????? ??? ?????
			var batches = tokenList
				.Select((token, index) => new { token, index })
				.GroupBy(x => x.index / _options.MaxBatchSize)
				.Select(g => g.Select(x => x.token).ToList())
				.ToList();

			var responses = new List<SendResponse>();
			var successCount = 0;
			var failureCount = 0;

			foreach (var batch in batches)
			{
				var batchMessage = new MulticastMessage
				{
					Tokens = batch,
					Notification = message.Notification,
					Data = message.Data,
					Android = message.Android,
					Apns = message.Apns,
					Webpush = message.Webpush
				};

				var batchResponse = await _messaging!.SendEachForMulticastAsync(
					batchMessage,
					_options.DryRun,
					cancellationToken);

				responses.AddRange(batchResponse.Responses);
				successCount += batchResponse.SuccessCount;
				failureCount += batchResponse.FailureCount;
			}

			return new CustomBatchResponse
			{
				SuccessCount = successCount,
				FailureCount = failureCount,
				Responses = responses
			};
		}

		try
		{
			_logger.LogInformation(
				"📤 [FCM] Sending multicast to {Count} tokens, DryRun={DryRun}, ProjectId={ProjectId}",
				tokenList.Count,
				_options.DryRun,
				_options.ProjectId);

			// طباعة التوكنات
			for (int i = 0; i < tokenList.Count; i++)
			{
				var t = tokenList[i];
				var masked = t.Length > 20 ? $"{t[..10]}...{t[^10..]}" : t;
				_logger.LogInformation("📱 [FCM] Token[{Index}]: {Token}", i, masked);
			}

			_logger.LogInformation(
				"📨 [FCM] Message: Title={Title}, Body={Body}",
				message.Notification?.Title ?? "(null)",
				message.Notification?.Body ?? "(null)");

			var response = await _messaging!.SendEachForMulticastAsync(
				message,
				false, // ⚠️ تعطيل DryRun للاختبار - إرسال حقيقي
				cancellationToken);

			_logger.LogInformation(
				"✅ [FCM] Response: Success={Success}, Failure={Failure}",
				response.SuccessCount,
				response.FailureCount);

			// طباعة تفاصيل كل استجابة
			for (int i = 0; i < response.Responses.Count; i++)
			{
				var r = response.Responses[i];
				if (r.IsSuccess)
				{
					_logger.LogInformation("✅ [FCM] Token[{Index}]: Success, MessageId={MessageId}", i, r.MessageId);
				}
				else
				{
					_logger.LogError("❌ [FCM] Token[{Index}]: Failed, Error={Error}", i, r.Exception?.Message);
				}
			}

			return new CustomBatchResponse
			{
				FailureCount = response.FailureCount,
				SuccessCount = response.SuccessCount,
				Responses = [.. response.Responses]
			};
		}
		catch (FirebaseMessagingException ex)
		{
			_logger.LogError(ex, "❌ [FCM] Firebase multicast error: {ErrorCode} - {Message}", ex.MessagingErrorCode, ex.Message);
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ [FCM] General error sending multicast");
			throw;
		}
	}

	/// <summary>
	/// ????? ????? ??? Topic
	/// </summary>
	public async Task<string> SendToTopicAsync(
		string topic,
		Message message,
		CancellationToken cancellationToken = default)
	{
		EnsureInitialized();

		try
		{
			_logger.LogDebug("Sending FCM message to topic: {Topic}", topic);

			var response = await _messaging!.SendAsync(message, _options.DryRun, cancellationToken);

			_logger.LogInformation(
				"FCM topic message sent successfully. Message ID: {MessageId}",
				response);

			return response;
		}
		catch (FirebaseMessagingException ex)
		{
			_logger.LogError(ex, "Firebase topic messaging error");
			throw;
		}
	}

	/// <summary>
	/// ???????? ?? Topic
	/// </summary>
	public async Task<TopicManagementResponse> SubscribeToTopicAsync(
		IReadOnlyList<string> tokens,
		string topic,
		CancellationToken cancellationToken = default)
	{
		EnsureInitialized();

		try
		{
			var response = await _messaging!.SubscribeToTopicAsync(
				tokens,
				topic);

			_logger.LogInformation(
				"Subscribed {Count} tokens to topic {Topic}. Success: {Success}, Failure: {Failure}",
				tokens.Count,
				topic,
				response.SuccessCount,
				response.FailureCount);

			return response;
		}
		catch (FirebaseMessagingException ex)
		{
			_logger.LogError(ex, "Firebase topic subscription error");
			throw;
		}
	}

	/// <summary>
	/// ????? ???????? ?? Topic
	/// </summary>
	public async Task<TopicManagementResponse> UnsubscribeFromTopicAsync(
		IReadOnlyList<string> tokens,
		string topic,
		CancellationToken cancellationToken = default)
	{
		EnsureInitialized();

		try
		{
			var response = await _messaging!.UnsubscribeFromTopicAsync(
				tokens,
				topic);

			_logger.LogInformation(
				"Unsubscribed {Count} tokens from topic {Topic}. Success: {Success}, Failure: {Failure}",
				tokens.Count,
				topic,
				response.SuccessCount,
				response.FailureCount);

			return response;
		}
		catch (FirebaseMessagingException ex)
		{
			_logger.LogError(ex, "Firebase topic unsubscription error");
			throw;
		}
	}

	private static string MaskToken(string token)
	{
		if (string.IsNullOrEmpty(token) || token.Length < 10)
			return "***";

		return $"{token[..5]}...{token[^5..]}";
	}
}

