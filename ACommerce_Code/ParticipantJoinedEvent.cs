using MediatR;

namespace ACommerce.Chats.Abstractions.Events;

/// <summary>
/// ??? ?????? ????? ????
/// </summary>
public class ParticipantJoinedEvent : INotification
{
	public Guid ChatId { get; set; }
	public string UserId { get; set; } = string.Empty;
	public DateTime JoinedAt { get; set; }
}

