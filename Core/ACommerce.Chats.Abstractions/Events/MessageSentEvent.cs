using MediatR;

namespace ACommerce.Chats.Abstractions.Events;

/// <summary>
/// ??? ????? ?????
/// </summary>
public class MessageSentEvent : INotification
{
	public Guid ChatId { get; set; }
	public Guid MessageId { get; set; }
	public string SenderId { get; set; } = string.Empty;
	public DateTime SentAt { get; set; }
}

