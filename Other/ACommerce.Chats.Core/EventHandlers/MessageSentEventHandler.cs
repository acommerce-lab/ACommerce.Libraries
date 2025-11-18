using ACommerce.Chats.Abstractions.Events;
using ACommerce.Chats.Abstractions.Providers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ACommerce.Chats.Core.EventHandlers;

/// <summary>
/// ????? ??? ????? ?????
/// ???? ??????? ??? Real-time ????????
/// </summary>
public class MessageSentEventHandler : INotificationHandler<MessageSentEvent>
{
	private readonly IRealtimeChatProvider _realtimeProvider;
	private readonly IMessageProvider _messageProvider;
	private readonly ILogger<MessageSentEventHandler> _logger;

	public MessageSentEventHandler(
		IRealtimeChatProvider realtimeProvider,
		IMessageProvider messageProvider,
		ILogger<MessageSentEventHandler> logger)
	{
		_realtimeProvider = realtimeProvider;
		_messageProvider = messageProvider;
		_logger = logger;
	}

	public async Task Handle(MessageSentEvent notification, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogDebug("Handling MessageSentEvent for message {MessageId}", notification.MessageId);

			// ??? ??????? ???????
			var message = await _messageProvider.GetMessageAsync(notification.MessageId, cancellationToken);

			if (message != null)
			{
				// ????? ??? Real-time
				await _realtimeProvider.SendMessageToChat(notification.ChatId, message, cancellationToken);

				_logger.LogInformation("Message {MessageId} sent via real-time", notification.MessageId);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error handling MessageSentEvent for message {MessageId}", notification.MessageId);
		}
	}
}

