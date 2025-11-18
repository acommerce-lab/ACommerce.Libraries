using ACommerce.Chats.Abstractions.Enums;

namespace ACommerce.Chats.Abstractions.DTOs;

/// <summary>
/// ?????? ????? ????? ?????
/// </summary>
public class CreateChatDto
{
	public required string Title { get; set; }
	public ChatType Type { get; set; } = ChatType.Group;
	public string? Description { get; set; }
	public string? ImageUrl { get; set; }
	public required string CreatorUserId { get; set; }
	public List<string> ParticipantUserIds { get; set; } = new();
}

