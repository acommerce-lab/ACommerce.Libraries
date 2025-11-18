using MediatR;

namespace ACommerce.Chats.Abstractions.Events;

/// <summary>
/// ??? ??? ?????
/// </summary>
public class MessageDeletedEvent : INotification
{
	public Guid ChatId { get; set; }
	public Guid MessageId { get; set; }
	public string DeletedByUserId { get; set; } = string.Empty;
	public DateTime DeletedAt { get; set; }
}

