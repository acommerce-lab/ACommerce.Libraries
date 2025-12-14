namespace ACommerce.Chats.Abstractions.DTOs;

/// <summary>
/// ?????? ???? ???????
/// </summary>
public class TypingIndicatorDto
{
	public Guid ChatId { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string UserName { get; set; } = string.Empty;
	public bool IsTyping { get; set; }
}

