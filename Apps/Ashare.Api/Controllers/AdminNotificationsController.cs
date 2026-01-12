using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Channels.Firebase.Storage;
using ACommerce.Data;
using Microsoft.EntityFrameworkCore;

namespace Ashare.Api.Controllers;

/// <summary>
/// API لإرسال الإشعارات من لوحة التحكم
/// </summary>
[Authorize]
[ApiController]
[Route("api/admin/notifications")]
[Produces("application/json")]
public class AdminNotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IFirebaseTokenStore? _firebaseTokenStore;
    private readonly ACommerceDbContext _dbContext;
    private readonly ILogger<AdminNotificationsController> _logger;

    public AdminNotificationsController(
        INotificationService notificationService,
        ACommerceDbContext dbContext,
        ILogger<AdminNotificationsController> logger,
        IFirebaseTokenStore? firebaseTokenStore = null)
    {
        _notificationService = notificationService;
        _dbContext = dbContext;
        _firebaseTokenStore = firebaseTokenStore;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على قائمة المستخدمين مع معلومات أجهزتهم
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<NotificationUserDto>), 200)]
    public async Task<IActionResult> GetUsersForNotification(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _dbContext.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(u =>
                    (u.FirstName != null && u.FirstName.ToLower().Contains(search)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(search)) ||
                    (u.Email != null && u.Email.ToLower().Contains(search)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new NotificationUserDto
                {
                    UserId = u.Id,
                    Name = $"{u.FirstName ?? ""} {u.LastName ?? ""}".Trim(),
                    Email = u.Email,
                    Phone = u.PhoneNumber,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            // إضافة معلومات الأجهزة المسجلة لكل مستخدم
            if (_firebaseTokenStore != null)
            {
                foreach (var user in users)
                {
                    var deviceCount = await _firebaseTokenStore.GetActiveDeviceCountAsync(user.UserId);
                    user.DeviceCount = deviceCount;
                    user.HasDevices = deviceCount > 0;
                }
            }

            return Ok(new
            {
                items = users,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users for notification");
            return StatusCode(500, new { message = "فشل في جلب المستخدمين" });
        }
    }

    /// <summary>
    /// إرسال إشعار لمستخدم واحد أو أكثر
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(SendNotificationResultDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "عنوان الإشعار مطلوب" });
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "نص الإشعار مطلوب" });
        }

        if (request.UserIds == null || !request.UserIds.Any())
        {
            return BadRequest(new { message = "يجب تحديد مستخدم واحد على الأقل" });
        }

        var results = new List<UserNotificationResult>();
        var successCount = 0;
        var failedCount = 0;

        foreach (var userId in request.UserIds)
        {
            try
            {
                // إرسال عبر القناتين: InApp (SignalR) + Firebase (Push)
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = request.Title,
                    Message = request.Message,
                    Type = MapNotificationType(request.Type),
                    Priority = MapPriority(request.Priority),
                    CreatedAt = DateTimeOffset.UtcNow,
                    ActionUrl = request.ActionUrl,
                    ImageUrl = request.ImageUrl,
                    Sound = "default",
                    BadgeCount = 1,
                    Channels = new List<ChannelDelivery>
                    {
                        new() { Channel = NotificationChannel.InApp },    // للتطبيق المفتوح
                        new() { Channel = NotificationChannel.Firebase }  // للتطبيق المغلق
                    },
                    Data = new Dictionary<string, string>
                    {
                        ["type"] = request.Type ?? "admin",
                        ["source"] = "admin_panel",
                        ["timestamp"] = DateTimeOffset.UtcNow.ToString("o")
                    }
                };

                // إضافة بيانات إضافية إن وجدت
                if (request.Data != null)
                {
                    foreach (var item in request.Data)
                    {
                        notification.Data[item.Key] = item.Value;
                    }
                }

                var result = await _notificationService.SendAsync(notification);

                results.Add(new UserNotificationResult
                {
                    UserId = userId,
                    Success = result.Success,
                    DeliveredChannels = result.DeliveredChannels,
                    FailedChannels = result.FailedChannels,
                    ErrorMessage = result.ErrorMessage
                });

                if (result.Success)
                    successCount++;
                else
                    failedCount++;

                _logger.LogInformation(
                    "Admin notification sent to user {UserId}: Success={Success}, Channels={Channels}",
                    userId, result.Success, string.Join(",", result.DeliveredChannels ?? []));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
                results.Add(new UserNotificationResult
                {
                    UserId = userId,
                    Success = false,
                    ErrorMessage = ex.Message
                });
                failedCount++;
            }
        }

        return Ok(new SendNotificationResultDto
        {
            TotalSent = request.UserIds.Count,
            SuccessCount = successCount,
            FailedCount = failedCount,
            Results = results
        });
    }

    /// <summary>
    /// إرسال إشعار لجميع المستخدمين (Broadcast)
    /// </summary>
    [HttpPost("broadcast")]
    [ProducesResponseType(typeof(SendNotificationResultDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "عنوان الإشعار مطلوب" });
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "نص الإشعار مطلوب" });
        }

        try
        {
            // جلب جميع المستخدمين الذين لديهم أجهزة مسجلة
            var userIds = await _dbContext.Users
                .Where(u => u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            if (!userIds.Any())
            {
                return Ok(new SendNotificationResultDto
                {
                    TotalSent = 0,
                    SuccessCount = 0,
                    FailedCount = 0,
                    Results = new List<UserNotificationResult>()
                });
            }

            // إرسال للجميع
            var sendRequest = new SendNotificationRequest
            {
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                Priority = request.Priority,
                ActionUrl = request.ActionUrl,
                ImageUrl = request.ImageUrl,
                Data = request.Data,
                UserIds = userIds
            };

            return await SendNotification(sendRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting notification");
            return StatusCode(500, new { message = "فشل في إرسال الإشعار الجماعي" });
        }
    }

    /// <summary>
    /// الحصول على إحصائيات الإشعارات
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(NotificationStatsDto), 200)]
    public async Task<IActionResult> GetNotificationStats()
    {
        try
        {
            var totalUsers = await _dbContext.Users.CountAsync();
            var activeUsers = await _dbContext.Users.CountAsync(u => u.IsActive);

            var usersWithDevices = 0;
            if (_firebaseTokenStore != null)
            {
                // هذا تقدير - في بيئة الإنتاج يمكن عمل query أفضل
                var sampleUsers = await _dbContext.Users.Take(100).Select(u => u.Id).ToListAsync();
                foreach (var userId in sampleUsers)
                {
                    var count = await _firebaseTokenStore.GetActiveDeviceCountAsync(userId);
                    if (count > 0) usersWithDevices++;
                }
                // تقدير النسبة
                usersWithDevices = (int)(usersWithDevices / 100.0 * totalUsers);
            }

            return Ok(new NotificationStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                UsersWithDevices = usersWithDevices
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification stats");
            return StatusCode(500, new { message = "فشل في جلب الإحصائيات" });
        }
    }

    private static NotificationType MapNotificationType(string? type) => type?.ToLower() switch
    {
        "info" => NotificationType.Info,
        "warning" => NotificationType.Warning,
        "error" => NotificationType.Error,
        "success" => NotificationType.Success,
        "promo" or "promotion" => NotificationType.Promotion,
        "system" => NotificationType.Welcome,
        _ => NotificationType.Info
    };

    private static NotificationPriority MapPriority(string? priority) => priority?.ToLower() switch
    {
        "low" => NotificationPriority.Low,
        "normal" => NotificationPriority.Normal,
        "high" => NotificationPriority.High,
        "urgent" or "critical" => NotificationPriority.Critical,
        _ => NotificationPriority.Normal
    };
}

// ============================================================================
// DTOs
// ============================================================================

public class NotificationUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int DeviceCount { get; set; }
    public bool HasDevices { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class SendNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; } = "info";
    public string? Priority { get; set; } = "normal";
    public string? ActionUrl { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
    public List<string> UserIds { get; set; } = new();
}

public class BroadcastNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; } = "info";
    public string? Priority { get; set; } = "normal";
    public string? ActionUrl { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}

public class SendNotificationResultDto
{
    public int TotalSent { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<UserNotificationResult> Results { get; set; } = new();
}

public class UserNotificationResult
{
    public string UserId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public IEnumerable<string>? DeliveredChannels { get; set; }
    public IEnumerable<string>? FailedChannels { get; set; }
    public string? ErrorMessage { get; set; }
}

public class NotificationStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int UsersWithDevices { get; set; }
}
