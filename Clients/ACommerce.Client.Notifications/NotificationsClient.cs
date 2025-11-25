using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Notifications;

public sealed class NotificationsClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Notifications"; // أو "Marketplace"

	public NotificationsClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// الحصول على الإشعارات
	/// </summary>
	public async Task<List<NotificationResponse>?> GetNotificationsAsync(
		int page = 1,
		int pageSize = 20,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<NotificationResponse>>(
			ServiceName,
			$"/api/notifications?page={page}&pageSize={pageSize}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على عدد الإشعارات غير المقروءة
	/// </summary>
	public async Task<UnreadCountResponse?> GetUnreadCountAsync(
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<UnreadCountResponse>(
			ServiceName,
			"/api/notifications/unread-count",
			cancellationToken);
	}

	/// <summary>
	/// تعليم إشعار كمقروء
	/// </summary>
	public async Task MarkAsReadAsync(
		Guid notificationId,
		CancellationToken cancellationToken = default)
	{
		await _httpClient.PostAsync<object>(
			ServiceName,
			$"/api/notifications/{notificationId}/read",
			new { },
			cancellationToken);
	}

	/// <summary>
	/// تعليم جميع الإشعارات كمقروءة
	/// </summary>
	public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
	{
		await _httpClient.PostAsync<object>(
			ServiceName,
			"/api/notifications/mark-all-read",
			new { },
			cancellationToken);
	}

	/// <summary>
	/// حذف إشعار
	/// </summary>
	public async Task DeleteNotificationAsync(
		Guid notificationId,
		CancellationToken cancellationToken = default)
	{
		await _httpClient.DeleteAsync(
			ServiceName,
			$"/api/notifications/{notificationId}",
			cancellationToken);
	}

	/// <summary>
	/// تسجيل Device Token للإشعارات Push
	/// </summary>
	public async Task RegisterDeviceTokenAsync(
		RegisterDeviceTokenRequest request,
		CancellationToken cancellationToken = default)
	{
		await _httpClient.PostAsync<RegisterDeviceTokenRequest>(
			ServiceName,
			"/api/notifications/device-token",
			request,
			cancellationToken);
	}
}

public sealed class NotificationResponse
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty; // "Order", "Payment", "Shipping", etc.
	public bool IsRead { get; set; }
	public DateTime CreatedAt { get; set; }
	public Dictionary<string, string> Data { get; set; } = new();
}

public sealed class UnreadCountResponse
{
	public int Count { get; set; }
}

public sealed class RegisterDeviceTokenRequest
{
	public string DeviceToken { get; set; } = string.Empty;
	public string Platform { get; set; } = string.Empty; // "iOS", "Android", "Web"
}
