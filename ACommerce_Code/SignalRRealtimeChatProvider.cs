using ACommerce.Chats.Abstractions.DTOs;
using ACommerce.Chats.Abstractions.Providers;
using ACommerce.Realtime.Abstractions.Contracts; // ? ??????? ??????? ????????!
using Microsoft.Extensions.Logging;

namespace ACommerce.Chats.Core.Providers;

/// <summary>
/// ???? Real-time ???????? ACommerce.Realtime! ??
/// </summary>
public class SignalRRealtimeChatProvider : IRealtimeChatProvider
{
	// ? ??????? IRealtimeHub ?? ACommerce.Realtime.Abstractions!
	private readonly IRealtimeHub _realtimeHub;
	private readonly ILogger<SignalRRealtimeChatProvider> _logger;

	public SignalRRealtimeChatProvider(
		IRealtimeHub realtimeHub,
		ILogger<SignalRRealtimeChatProvider> logger)
	{
		_realtimeHub = realtimeHub ?? throw new ArgumentNullException(nameof(realtimeHub));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task SendMessageToChat(
		Guid chatId,
		MessageDto message,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending real-time message to chat {ChatId}", chatId);

		// ? ??????? IRealtimeHub.SendToGroupAsync
		await _realtimeHub.SendToGroupAsync(
			groupName: $"chat_{chatId}",
			method: "MessageReceived",
			data: message,
			cancellationToken: cancellationToken);
	}

	public async Task SendTypingIndicator(
		Guid chatId,
		TypingIndicatorDto indicator,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending typing indicator for {UserId} in chat {ChatId}",
			indicator.UserId, chatId);

		// ? ??????? IRealtimeHub.SendToGroupAsync
		await _realtimeHub.SendToGroupAsync(
			groupName: $"chat_{chatId}",
			method: "UserTyping",
			data: indicator,
			cancellationToken: cancellationToken);
	}

	public async Task SendParticipantJoined(
		Guid chatId,
		ParticipantDto participant,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending participant joined notification for {UserId} in chat {ChatId}",
			participant.UserId, chatId);

		// ? ??????? IRealtimeHub.SendToGroupAsync
		await _realtimeHub.SendToGroupAsync(
			groupName: $"chat_{chatId}",
			method: "ParticipantJoined",
			data: participant,
			cancellationToken: cancellationToken);
	}

	public async Task SendParticipantLeft(
		Guid chatId,
		string userId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending participant left notification for {UserId} in chat {ChatId}",
			userId, chatId);

		// ? ??????? IRealtimeHub.SendToGroupAsync
		await _realtimeHub.SendToGroupAsync(
			groupName: $"chat_{chatId}",
			method: "ParticipantLeft",
			data: new { ChatId = chatId, UserId = userId },
			cancellationToken: cancellationToken);
	}

	public async Task SendUserPresenceUpdate(
		string userId,
		bool isOnline,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending presence update for {UserId}: {IsOnline}", userId, isOnline);

		// ? ??????? IRealtimeHub.SendToAllAsync
		await _realtimeHub.SendToAllAsync(
			method: "UserPresenceChanged",
			data: new { UserId = userId, IsOnline = isOnline },
			cancellationToken: cancellationToken);
	}

	public async Task SendMessageRead(
		Guid chatId,
		string userId,
		Guid messageId,
		CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Sending message read notification for {MessageId} by {UserId}",
			messageId, userId);

		// ? ??????? IRealtimeHub.SendToGroupAsync
		await _realtimeHub.SendToGroupAsync(
			groupName: $"chat_{chatId}",
			method: "MessageRead",
			data: new { ChatId = chatId, UserId = userId, MessageId = messageId },
			cancellationToken: cancellationToken);
	}
}

