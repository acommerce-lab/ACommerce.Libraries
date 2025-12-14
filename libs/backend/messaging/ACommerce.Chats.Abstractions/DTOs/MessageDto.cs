using ACommerce.Chats.Abstractions.Enums;

namespace ACommerce.Chats.Abstractions.DTOs;

/// <summary>
/// ?????? ???????
/// </summary>
public class MessageDto
{
	public Guid Id { get; set; }
	public Guid ChatId { get; set; }
	public string SenderId { get; set; } = string.Empty;
	public string SenderName { get; set; } = string.Empty;
	public string? SenderAvatar { get; set; }
	public string Content { get; set; } = string.Empty;
	public MessageType Type { get; set; }
	public Guid? ReplyToMessageId { get; set; }
	public MessageDto? ReplyToMessage { get; set; }
	public List<string> Attachments { get; set; } = new();
	public bool IsEdited { get; set; }
	public DateTime? EditedAt { get; set; }
	public int ReadByCount { get; set; }
	public DateTime CreatedAt { get; set; }
}

