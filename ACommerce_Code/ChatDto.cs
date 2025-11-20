using ACommerce.Chats.Abstractions.Enums;

namespace ACommerce.Chats.Abstractions.DTOs;

/// <summary>
/// ?????? ???????
/// </summary>
public class ChatDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public ChatType Type { get; set; }
	public string? Description { get; set; }
	public string? ImageUrl { get; set; }
	public int ParticipantsCount { get; set; }
	public int UnreadMessagesCount { get; set; }
	public MessageDto? LastMessage { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

