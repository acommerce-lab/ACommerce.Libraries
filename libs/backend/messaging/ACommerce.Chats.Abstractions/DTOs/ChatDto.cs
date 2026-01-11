using ACommerce.Chats.Abstractions.Enums;

namespace ACommerce.Chats.Abstractions.DTOs;

/// <summary>
/// بيانات المحادثة
/// </summary>
public class ChatDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public ChatType Type { get; set; }
	public string? Description { get; set; }
	public string? ImageUrl { get; set; }

	/// <summary>
	/// معرف الطرف الآخر في المحادثات المباشرة
	/// </summary>
	public string? OtherPartyId { get; set; }

	/// <summary>
	/// اسم الطرف الآخر (يُملأ من خدمة البروفايل)
	/// </summary>
	public string? OtherPartyName { get; set; }

	/// <summary>
	/// صورة الطرف الآخر
	/// </summary>
	public string? OtherPartyAvatar { get; set; }

	/// <summary>
	/// قائمة معرفات المشاركين
	/// </summary>
	public List<string> Participants { get; set; } = new();

	public int ParticipantsCount { get; set; }
	public int UnreadMessagesCount { get; set; }
	public MessageDto? LastMessage { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

