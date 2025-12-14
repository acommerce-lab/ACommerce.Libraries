using MediatR;

namespace ACommerce.Chats.Abstractions.Events;

/// <summary>
/// ??? ????? ?????
/// </summary>
public class MessageReadEvent : INotification
{
	public Guid ChatId { get; set; }
	public Guid MessageId { get; set; }
	public string UserId { get; set; } = string.Empty;
	public DateTime ReadAt { get; set; }
}

