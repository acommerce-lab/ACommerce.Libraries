using ACommerce.Chats.Abstractions.Enums;

namespace ACommerce.Chats.Abstractions.DTOs;

/// <summary>
/// ?????? ???????
/// </summary>
public class ParticipantDto
{
	public Guid Id { get; set; }
	public Guid ChatId { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string UserName { get; set; } = string.Empty;
	public string? UserAvatar { get; set; }
	public ParticipantRole Role { get; set; }
	public bool IsOnline { get; set; }
	public DateTime? LastSeenAt { get; set; }
	public Guid? LastSeenMessageId { get; set; }
	public int UnreadMessagesCount { get; set; }
	public bool IsMuted { get; set; }
	public bool IsPinned { get; set; }
	public DateTime JoinedAt { get; set; }
}

