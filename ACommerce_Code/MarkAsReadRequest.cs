namespace ACommerce.Chats.Abstractions.DTOs;

/// <summary>
/// ??? ????? ??????
/// </summary>
public class MarkAsReadRequest
{
	public required string UserId { get; set; }
	public Guid? LastMessageId { get; set; }
}

