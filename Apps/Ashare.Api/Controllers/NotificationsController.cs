using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Ashare.Api.Controllers;

/// <summary>
/// نقاط نهاية الإشعارات لتطبيق عشير
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
	// In-memory storage for demo purposes
	private static readonly ConcurrentDictionary<Guid, NotificationDto> _notifications = new();
	private static bool _isSeeded = false;

	public NotificationsController()
	{
		SeedNotificationsIfNeeded();
	}

	private static void SeedNotificationsIfNeeded()
	{
		if (_isSeeded) return;
		_isSeeded = true;

		var sampleNotifications = new List<NotificationDto>
		{
			new()
			{
				Id = Guid.NewGuid(),
				Title = "مرحباً بك في عشير!",
				Message = "شكراً لانضمامك إلينا. ابدأ باستكشاف المساحات المتاحة للإيجار.",
				Type = "System",
				IsRead = false,
				CreatedAt = DateTime.UtcNow.AddMinutes(-5),
				UserId = "demo-user"
			},
			new()
			{
				Id = Guid.NewGuid(),
				Title = "عرض جديد متاح",
				Message = "شقة مفروشة في حي النرجس متاحة الآن. تحقق من التفاصيل!",
				Type = "Promo",
				IsRead = false,
				CreatedAt = DateTime.UtcNow.AddHours(-2),
				UserId = "demo-user"
			},
			new()
			{
				Id = Guid.NewGuid(),
				Title = "تم تأكيد حجزك",
				Message = "تم تأكيد حجزك لقاعة الاجتماعات VIP ليوم الأحد القادم.",
				Type = "Booking",
				IsRead = true,
				CreatedAt = DateTime.UtcNow.AddDays(-1),
				UserId = "demo-user"
			},
			new()
			{
				Id = Guid.NewGuid(),
				Title = "رسالة جديدة",
				Message = "لديك رسالة جديدة من مالك الشقة بخصوص استفسارك.",
				Type = "Message",
				IsRead = false,
				CreatedAt = DateTime.UtcNow.AddHours(-6),
				UserId = "demo-user"
			},
			new()
			{
				Id = Guid.NewGuid(),
				Title = "تم استلام الدفع",
				Message = "تم استلام دفعة الإيجار بنجاح. شكراً لك!",
				Type = "Payment",
				IsRead = true,
				CreatedAt = DateTime.UtcNow.AddDays(-3),
				UserId = "demo-user"
			},
			new()
			{
				Id = Guid.NewGuid(),
				Title = "تقييم جديد",
				Message = "حصلت على تقييم 5 نجوم من أحمد على مساحتك.",
				Type = "Review",
				IsRead = false,
				CreatedAt = DateTime.UtcNow.AddHours(-12),
				UserId = "demo-user"
			},
			new()
			{
				Id = Guid.NewGuid(),
				Title = "تذكير بالحجز",
				Message = "تذكير: حجزك للمكتب المشترك يبدأ غداً الساعة 9 صباحاً.",
				Type = "Booking",
				IsRead = false,
				CreatedAt = DateTime.UtcNow.AddHours(-1),
				UserId = "demo-user"
			}
		};

		foreach (var notification in sampleNotifications)
		{
			_notifications.TryAdd(notification.Id, notification);
		}
	}

	/// <summary>
	/// الحصول على جميع الإشعارات
	/// </summary>
	[HttpGet]
	public ActionResult<List<NotificationDto>> GetNotifications(
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 20)
	{
		var notifications = _notifications.Values
			.OrderByDescending(n => n.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();

		return Ok(notifications);
	}

	/// <summary>
	/// الحصول على إشعاراتي
	/// </summary>
	[HttpGet("me")]
	public ActionResult<List<NotificationDto>> GetMyNotifications()
	{
		var notifications = _notifications.Values
			.OrderByDescending(n => n.CreatedAt)
			.ToList();

		return Ok(notifications);
	}

	/// <summary>
	/// الحصول على عدد الإشعارات غير المقروءة
	/// </summary>
	[HttpGet("unread-count")]
	public ActionResult<object> GetUnreadCount()
	{
		var count = _notifications.Values.Count(n => !n.IsRead);
		return Ok(new { count });
	}

	/// <summary>
	/// تعليم إشعار كمقروء
	/// </summary>
	[HttpPost("{id}/read")]
	public ActionResult MarkAsRead(Guid id)
	{
		if (_notifications.TryGetValue(id, out var notification))
		{
			notification.IsRead = true;
			return Ok();
		}

		return NotFound();
	}

	/// <summary>
	/// تعليم جميع الإشعارات كمقروءة
	/// </summary>
	[HttpPost("mark-all-read")]
	public ActionResult MarkAllAsRead()
	{
		foreach (var notification in _notifications.Values)
		{
			notification.IsRead = true;
		}

		return Ok();
	}

	/// <summary>
	/// حذف إشعار
	/// </summary>
	[HttpDelete("{id}")]
	public ActionResult DeleteNotification(Guid id)
	{
		if (_notifications.TryRemove(id, out _))
		{
			return Ok();
		}

		return NotFound();
	}

	/// <summary>
	/// تسجيل Device Token
	/// </summary>
	[HttpPost("device-token")]
	public ActionResult RegisterDeviceToken([FromBody] RegisterDeviceTokenDto request)
	{
		// In a real app, this would store the token for push notifications
		return Ok();
	}

	/// <summary>
	/// الحصول على إعدادات Push
	/// </summary>
	[HttpGet("push-settings")]
	public ActionResult<PushSettingsDto> GetPushSettings()
	{
		return Ok(new PushSettingsDto
		{
			EnablePush = true,
			OrderUpdates = true,
			ChatMessages = true,
			Promotions = true,
			SystemAlerts = true
		});
	}

	/// <summary>
	/// تحديث إعدادات Push
	/// </summary>
	[HttpPut("push-settings")]
	public ActionResult UpdatePushSettings([FromBody] PushSettingsDto settings)
	{
		// In a real app, this would update user preferences
		return Ok();
	}
}

public class NotificationDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
	public string Type { get; set; } = "System";
	public bool IsRead { get; set; }
	public DateTime CreatedAt { get; set; }
	public string UserId { get; set; } = string.Empty;
	public Dictionary<string, string> Data { get; set; } = new();
}

public class RegisterDeviceTokenDto
{
	public string DeviceToken { get; set; } = string.Empty;
	public string Platform { get; set; } = string.Empty;
}

public class PushSettingsDto
{
	public bool EnablePush { get; set; }
	public bool OrderUpdates { get; set; } = true;
	public bool ChatMessages { get; set; } = true;
	public bool Promotions { get; set; } = true;
	public bool SystemAlerts { get; set; } = true;
}
