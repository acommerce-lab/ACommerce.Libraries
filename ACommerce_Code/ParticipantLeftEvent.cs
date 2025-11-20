using MediatR;

namespace ACommerce.Chats.Abstractions.Events;

/// <summary>
/// ??? ?????? ?????
/// </summary>
public class ParticipantLeftEvent : INotification
{
	public Guid ChatId { get; set; }
	public string UserId { get; set; } = string.Empty;
	public DateTime LeftAt { get; set; }
}

