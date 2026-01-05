using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Complaints.Entities;

/// <summary>
/// شكوى أو اقتراح أو تذكرة دعم
/// </summary>
public class Complaint : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// معرف المستخدم صاحب الشكوى
	/// </summary>
	public required string UserId { get; set; }

	/// <summary>
	/// رقم التذكرة (للعرض)
	/// </summary>
	public required string TicketNumber { get; set; }

	/// <summary>
	/// نوع التذكرة: Complaint, Suggestion, Inquiry, Refund
	/// </summary>
	public required string Type { get; set; }

	/// <summary>
	/// العنوان
	/// </summary>
	public required string Title { get; set; }

	/// <summary>
	/// الوصف التفصيلي
	/// </summary>
	public required string Description { get; set; }

	/// <summary>
	/// الحالة: Open, InProgress, Resolved, Closed
	/// </summary>
	public string Status { get; set; } = "Open";

	/// <summary>
	/// الأولوية: Low, Medium, High, Urgent
	/// </summary>
	public string Priority { get; set; } = "Medium";

	/// <summary>
	/// التصنيف: Order, Product, Shipping, Payment, Account, Other
	/// </summary>
	public string Category { get; set; } = "Other";

	/// <summary>
	/// نوع الكيان المرتبط (Order, Product, Vendor, etc)
	/// </summary>
	public string? RelatedEntityType { get; set; }

	/// <summary>
	/// معرف الكيان المرتبط
	/// </summary>
	public Guid? RelatedEntityId { get; set; }

	/// <summary>
	/// المرفقات (روابط الصور/الملفات)
	/// </summary>
	[NotMapped]
	public List<string> Attachments { get; set; } = [];

	/// <summary>
	/// معرف الموظف المسؤول
	/// </summary>
	public string? AssignedToId { get; set; }

	/// <summary>
	/// اسم الموظف المسؤول
	/// </summary>
	public string? AssignedToName { get; set; }

	/// <summary>
	/// تاريخ الإغلاق
	/// </summary>
	public DateTime? ClosedAt { get; set; }

	/// <summary>
	/// تقييم المستخدم للحل (1-5)
	/// </summary>
	public int? UserRating { get; set; }

	/// <summary>
	/// ملاحظات المستخدم على الحل
	/// </summary>
	public string? UserFeedback { get; set; }

	/// <summary>
	/// بيانات إضافية
	/// </summary>
	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = [];
}

/// <summary>
/// رد على الشكوى
/// </summary>
public class ComplaintReply : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// معرف الشكوى
	/// </summary>
	public required Guid ComplaintId { get; set; }

	/// <summary>
	/// معرف المرسل
	/// </summary>
	public required string SenderId { get; set; }

	/// <summary>
	/// اسم المرسل
	/// </summary>
	public required string SenderName { get; set; }

	/// <summary>
	/// هل المرسل موظف؟
	/// </summary>
	public bool IsStaff { get; set; }

	/// <summary>
	/// نص الرد
	/// </summary>
	public required string Message { get; set; }

	/// <summary>
	/// المرفقات
	/// </summary>
	[NotMapped]
	public List<string> Attachments { get; set; } = [];

	/// <summary>
	/// هل هو رد داخلي (للموظفين فقط)؟
	/// </summary>
	public bool IsInternal { get; set; }
}
