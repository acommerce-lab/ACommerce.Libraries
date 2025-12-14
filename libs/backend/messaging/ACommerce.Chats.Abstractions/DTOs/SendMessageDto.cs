using ACommerce.Chats.Abstractions.Enums;

namespace ACommerce.Chats.Abstractions.DTOs;

/// <summary>
/// ?????? ????? ?????
/// </summary>
public class SendMessageDto
{
	public required string SenderId { get; set; }
	public required string Content { get; set; }
	public MessageType Type { get; set; } = MessageType.Text;
	public Guid? ReplyToMessageId { get; set; }
	public List<string>? Attachments { get; set; }
}

