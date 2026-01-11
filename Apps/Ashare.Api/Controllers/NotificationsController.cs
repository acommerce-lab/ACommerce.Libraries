using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Channels.Firebase.Storage;
using ACommerce.Notifications.Channels.Firebase.Models;
using ACommerce.SharedKernel.Abstractions.Queries;
using System.Security.Claims;

namespace Ashare.Api.Controllers;

/// <summary>
/// نقاط نهاية الإشعارات لتطبيق عشير
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IFirebaseTokenStore? _firebaseTokenStore;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger,
        IFirebaseTokenStore? firebaseTokenStore = null)
    {
        _notificationService = notificationService;
        _firebaseTokenStore = firebaseTokenStore;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// تسجيل Device Token للإشعارات
    /// </summary>
    [HttpPost("device-token")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterDeviceTokenRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        if (string.IsNullOrEmpty(request.DeviceToken))
        {
            return BadRequest(new { message = "Device token is required" });
        }

        if (_firebaseTokenStore == null)
        {
            _logger.LogWarning("Firebase token store not configured");
            return Ok(new { message = "Firebase not configured, token not saved" });
        }

        try
        {
            // تحديد نوع الجهاز
            var platform = request.Platform?.ToLowerInvariant() switch
            {
                "ios" => DevicePlatform.iOS,
                "android" => DevicePlatform.Android,
                "web" => DevicePlatform.Web,
                _ => DevicePlatform.Android // الافتراضي
            };

            var deviceToken = new FirebaseDeviceToken
            {
                UserId = userId,
                Token = request.DeviceToken,
                Platform = platform,
                RegisteredAt = DateTimeOffset.UtcNow,
                LastUsedAt = DateTimeOffset.UtcNow,
                IsActive = true,
                Metadata = new Dictionary<string, string>
                {
                    ["AppVersion"] = request.AppVersion ?? "unknown",
                    ["DeviceModel"] = request.DeviceModel ?? "unknown"
                }
            };

            await _firebaseTokenStore.SaveTokenAsync(deviceToken);

            _logger.LogInformation("Device token registered for user {UserId}, platform: {Platform}",
                userId, platform);

            return Ok(new { success = true, message = "Device token registered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device token for user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to register device token" });
        }
    }

    /// <summary>
    /// إلغاء تسجيل Device Token
    /// </summary>
    [HttpDelete("device-token")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UnregisterDeviceToken([FromQuery] string deviceToken)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        if (_firebaseTokenStore == null)
        {
            return Ok(new { message = "Firebase not configured" });
        }

        try
        {
            await _firebaseTokenStore.DeleteTokenAsync(deviceToken);
            _logger.LogInformation("Device token unregistered for user {UserId}", userId);
            return Ok(new { success = true, message = "Device token unregistered" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering device token");
            return StatusCode(500, new { message = "Failed to unregister device token" });
        }
    }

    /// <summary>
    /// الحصول على عدد الأجهزة المسجلة
    /// </summary>
    [HttpGet("devices/count")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetDeviceCount()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        if (_firebaseTokenStore == null)
        {
            return Ok(new { count = 0 });
        }

        try
        {
            var count = await _firebaseTokenStore.GetActiveDeviceCountAsync(userId);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device count for user {UserId}", userId);
            return Ok(new { count = 0 });
        }
    }

    /// <summary>
    /// إرسال إشعار اختباري
    /// </summary>
    [HttpPost("test")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> SendTestNotification()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "إشعار اختباري",
                Message = "هذا إشعار اختباري من تطبيق عشير 🎉",
                Type = NotificationType.Info,
                Priority = NotificationPriority.Normal,
                CreatedAt = DateTimeOffset.UtcNow,
                Channels = new List<ChannelDelivery>
                {
                    new() { Channel = NotificationChannel.InApp },
                    new() { Channel = NotificationChannel.Firebase }
                },
                Data = new Dictionary<string, string>
                {
                    ["type"] = "test",
                    ["timestamp"] = DateTimeOffset.UtcNow.ToString("o")
                }
            };

            var result = await _notificationService.SendAsync(notification);

            _logger.LogInformation("Test notification sent to user {UserId}, success: {Success}",
                userId, result.Success);

            return Ok(new
            {
                success = result.Success,
                message = "تم إرسال الإشعار الاختباري",
                result = new
                {
                    success = result.Success,
                    errorMessage = result.ErrorMessage,
                    deliveredChannels = result.DeliveredChannels,
                    failedChannels = result.FailedChannels
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test notification to user {UserId}", userId);
            return StatusCode(500, new { message = "Failed to send test notification", error = ex.Message });
        }
    }

    /// <summary>
    /// الحصول على إعدادات الإشعارات
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(NotificationSettingsDto), 200)]
    public IActionResult GetNotificationSettings()
    {
        // في المستقبل يمكن تخزين هذه الإعدادات في قاعدة البيانات
        return Ok(new NotificationSettingsDto
        {
            EnablePush = true,
            NewBookings = true,
            BookingUpdates = true,
            ChatMessages = true,
            Promotions = true,
            SystemAlerts = true
        });
    }

    /// <summary>
    /// تحديث إعدادات الإشعارات
    /// </summary>
    [HttpPut("settings")]
    [ProducesResponseType(200)]
    public IActionResult UpdateNotificationSettings([FromBody] NotificationSettingsDto settings)
    {
        // TODO: حفظ الإعدادات في قاعدة البيانات
        _logger.LogInformation("Notification settings updated for user {UserId}", GetUserId());
        return Ok(new { success = true, message = "تم تحديث الإعدادات" });
    }
}

// ============================================================================
// DTOs
// ============================================================================

/// <summary>
/// طلب تسجيل Device Token
/// </summary>
public class RegisterDeviceTokenRequest
{
    /// <summary>
    /// Firebase Device Token
    /// </summary>
    public string DeviceToken { get; set; } = string.Empty;

    /// <summary>
    /// نوع الجهاز: ios, android, web
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// إصدار التطبيق
    /// </summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// موديل الجهاز
    /// </summary>
    public string? DeviceModel { get; set; }
}

/// <summary>
/// إعدادات الإشعارات
/// </summary>
public class NotificationSettingsDto
{
    public bool EnablePush { get; set; } = true;
    public bool NewBookings { get; set; } = true;
    public bool BookingUpdates { get; set; } = true;
    public bool ChatMessages { get; set; } = true;
    public bool Promotions { get; set; } = true;
    public bool SystemAlerts { get; set; } = true;
}
