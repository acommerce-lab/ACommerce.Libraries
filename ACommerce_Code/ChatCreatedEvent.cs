using MediatR;

namespace ACommerce.Chats.Abstractions.Events;

/// <summary>
/// ??? ????? ????? ?????
/// </summary>
public class ChatCreatedEvent : INotification
{
	public Guid ChatId { get; set; }
	public string CreatorUserId { get; set; } = string.Empty;
	public List<string> ParticipantUserIds { get; set; } = new();
	public DateTime CreatedAt { get; set; }
}

