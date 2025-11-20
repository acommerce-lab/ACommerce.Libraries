using ACommerce.Chats.Abstractions.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Chats.Core.Entities;

/// <summary>
/// ???? ??????? - ?????? IBaseEntity ?? SharedKernel! ?
/// </summary>
public class ChatParticipant : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	public Guid ChatId { get; set; }
	public Chat? Chat { get; set; }

	public required string UserId { get; set; }
	public ParticipantRole Role { get; set; } = ParticipantRole.Member;

	public DateTime? LastSeenMessageAt { get; set; }
	public Guid? LastSeenMessageId { get; set; }

	public bool IsMuted { get; set; }
	public bool IsPinned { get; set; }

	public Dictionary<string, string> Metadata { get; set; } = new();
}


