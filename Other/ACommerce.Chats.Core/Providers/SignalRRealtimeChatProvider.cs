using ACommerce.Chats.Abstractions.DTOs;
using ACommerce.Chats.Abstractions.Providers;
using ACommerce.Chats.Core.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ACommerce.Chats.Core.Providers;

/// <summary>
/// مزود Real-time للدردشة باستخدام ChatHub مباشرة
/// </summary>
public class SignalRRealtimeChatProvider : IRealtimeChatProvider
{
	// استخدام IHubContext للـ ChatHub مباشرة بدلاً من IRealtimeHub
	private readonly IHubContext<ChatHub, IChatClient> _hubContext;
	private readonly ILogger<SignalRRealtimeChatProvider> _logger;

	public SignalRRealtimeChatProvider(
		IHubContext<ChatHub, IChatClient> hubContext,
		ILogger<SignalRRealtimeChatProvider> logger)
	{
		_hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task SendMessageToChat(
		Guid chatId,
		MessageDto message,
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("📤 Sending real-time message to chat {ChatId}, MessageId: {MessageId}",
			chatId, message.Id);

		// إرسال للمجموعة chat_{chatId}
		await _hubContext.Clients
			.Group($"chat_{chatId}")
			.ReceiveMessage("MessageReceived", message);

		_logger.LogInformation("✅ Message sent to group chat_{ChatId}", chatId);
	}

	public async Task SendTypingIndicator(
		Guid chatId,
		TypingIndicatorDto indicator,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending typing indicator for {UserId} in chat {ChatId}",
			indicator.UserId, chatId);

		await _hubContext.Clients
			.Group($"chat_{chatId}")
			.ReceiveMessage("UserTyping", indicator);
	}

	public async Task SendParticipantJoined(
		Guid chatId,
		ParticipantDto participant,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending participant joined notification for {UserId} in chat {ChatId}",
			participant.UserId, chatId);

		await _hubContext.Clients
			.Group($"chat_{chatId}")
			.ReceiveMessage("ParticipantJoined", participant);
	}

	public async Task SendParticipantLeft(
		Guid chatId,
		string userId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending participant left notification for {UserId} in chat {ChatId}",
			userId, chatId);

		await _hubContext.Clients
			.Group($"chat_{chatId}")
			.ReceiveMessage("ParticipantLeft", new { ChatId = chatId, UserId = userId });
	}

	public async Task SendUserPresenceUpdate(
		string userId,
		bool isOnline,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending presence update for {UserId}: {IsOnline}", userId, isOnline);

		await _hubContext.Clients.All
			.ReceiveMessage("UserPresenceChanged", new { UserId = userId, IsOnline = isOnline });
	}

	public async Task SendMessageRead(
		Guid chatId,
		string userId,
		Guid messageId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending message read notification for {MessageId} by {UserId}",
			messageId, userId);

		await _hubContext.Clients
			.Group($"chat_{chatId}")
			.ReceiveMessage("MessageRead", new { ChatId = chatId, UserId = userId, MessageId = messageId });
	}
}
