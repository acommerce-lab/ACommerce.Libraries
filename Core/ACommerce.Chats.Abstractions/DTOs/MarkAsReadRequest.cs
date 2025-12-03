namespace ACommerce.Chats.Abstractions.DTOs;

/// <summary>
/// طلب تعليم كمقروء
/// </summary>
public class MarkAsReadRequest
{
	/// <summary>
	/// معرف المستخدم - اختياري، يُستخرج من JWT token إذا لم يُرسل
	/// </summary>
	public string? UserId { get; set; }

	/// <summary>
	/// معرف آخر رسالة مقروءة
	/// </summary>
	public Guid? LastMessageId { get; set; }
}

