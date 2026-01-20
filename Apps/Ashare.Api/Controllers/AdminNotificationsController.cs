using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Channels.Firebase.Storage;
using ACommerce.Notifications.Channels.Firebase.EntityFramework.Entities;
using ACommerce.Notifications.Channels.Firebase.Services;
using ACommerce.Profiles.Entities;
using Microsoft.EntityFrameworkCore;
using FirebaseAdmin.Messaging;

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
    private readonly FirebaseMessagingService? _firebaseMessagingService;
    private readonly DbContext _dbContext;
    private readonly ILogger<AdminNotificationsController> _logger;

    public AdminNotificationsController(
        INotificationService notificationService,
        DbContext dbContext,
        ILogger<AdminNotificationsController> logger,
        IFirebaseTokenStore? firebaseTokenStore = null,
        FirebaseMessagingService? firebaseMessagingService = null)
    {
        _notificationService = notificationService;
        _dbContext = dbContext;
        _firebaseTokenStore = firebaseTokenStore;
        _firebaseMessagingService = firebaseMessagingService;
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
            // 1. جلب جميع المستخدمين من Profiles (المصدر الرئيسي)
            var profilesQuery = _dbContext.Set<Profile>()
                .Where(p => !p.IsDeleted && p.IsActive);

            // تطبيق البحث
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                profilesQuery = profilesQuery.Where(p =>
                    (p.FullName != null && p.FullName.ToLower().Contains(search)) ||
                    (p.Email != null && p.Email.ToLower().Contains(search)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(search)));
            }

            var totalCount = await profilesQuery.CountAsync();

            // 2. جلب البروفايلات مع الترقيم
            var profiles = await profilesQuery
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    p.FullName,
                    p.Email,
                    p.PhoneNumber,
                    p.CreatedAt
                })
                .ToListAsync();

            // 3. جلب عدد الأجهزة لكل مستخدم من DeviceTokenEntity
            var userIds = profiles.Select(p => p.UserId ?? p.Id.ToString()).ToList();

            var deviceCounts = await _dbContext.Set<DeviceTokenEntity>()
                .Where(d => d.IsActive && !d.IsDeleted && userIds.Contains(d.UserId))
                .GroupBy(d => d.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            // 4. بناء النتيجة
            var users = profiles.Select(p =>
            {
                var userId = p.UserId ?? p.Id.ToString();
                deviceCounts.TryGetValue(userId, out var deviceCount);

                return new NotificationUserDto
                {
                    UserId = userId,
                    Name = p.FullName ?? "مستخدم",
                    Email = p.Email,
                    Phone = p.PhoneNumber,
                    DeviceCount = deviceCount,
                    HasDevices = deviceCount > 0,
                    CreatedAt = p.CreatedAt
                };
            }).ToList();

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
            // جلب جميع المستخدمين الذين لديهم أجهزة مسجلة فعلياً
            var userIds = await _dbContext.Set<DeviceTokenEntity>()
                .Where(d => d.IsActive && !d.IsDeleted)
                .Select(d => d.UserId)
                .Distinct()
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
            // إحصائيات البروفايلات (المصدر الرئيسي)
            var profileQuery = _dbContext.Set<Profile>().Where(p => !p.IsDeleted);
            var totalProfiles = await profileQuery.CountAsync();
            var activeProfiles = await profileQuery.CountAsync(p => p.IsActive);

            // إحصائيات الأجهزة
            var deviceQuery = _dbContext.Set<DeviceTokenEntity>().Where(d => !d.IsDeleted);
            var totalActiveDevices = await deviceQuery.CountAsync(d => d.IsActive);
            var usersWithDevices = await deviceQuery
                .Where(d => d.IsActive)
                .Select(d => d.UserId)
                .Distinct()
                .CountAsync();

            return Ok(new NotificationStatsDto
            {
                TotalUsers = totalProfiles,
                ActiveUsers = activeProfiles,
                UsersWithDevices = usersWithDevices,
                TotalActiveDevices = totalActiveDevices
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification stats");
            return StatusCode(500, new { message = "فشل في جلب الإحصائيات", error = ex.Message });
        }
    }

    /// <summary>
    /// اختبار الاتصال بقاعدة البيانات (بدون مصادقة)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("test")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            var profileCount = await _dbContext.Set<Profile>().CountAsync();
            var deviceCount = await _dbContext.Set<DeviceTokenEntity>().CountAsync();

            return Ok(new
            {
                success = true,
                message = "الاتصال يعمل",
                profilesCount = profileCount,
                devicesCount = deviceCount
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                success = false,
                message = "فشل الاتصال",
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }

    /// <summary>
    /// 🔥 اختبار إرسال إشعار Firebase مباشرة (للتشخيص)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("test-firebase")]
    public async Task<IActionResult> TestFirebaseSend([FromBody] TestFirebaseRequest request)
    {
        var diagnostics = new List<string>();

        try
        {
            diagnostics.Add("🚀 بدء الاختبار...");

            // 1. التحقق من وجود التوكن
            if (string.IsNullOrEmpty(request.Token))
            {
                // جلب أول توكن من قاعدة البيانات
                var firstToken = await _dbContext.Set<DeviceTokenEntity>()
                    .Where(d => d.IsActive && !d.IsDeleted)
                    .FirstOrDefaultAsync();

                if (firstToken == null)
                {
                    return Ok(new {
                        success = false,
                        diagnostics,
                        error = "لا توجد توكنات في قاعدة البيانات"
                    });
                }

                request.Token = firstToken.Token;
                request.UserId = firstToken.UserId;
                diagnostics.Add($"📱 تم جلب توكن من قاعدة البيانات للمستخدم: {request.UserId}");
            }

            diagnostics.Add($"📱 التوكن: {request.Token[..15]}...{request.Token[^10..]}");

            // 2. التحقق من إعدادات Firebase
            var firebaseKeyJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");
            var hasFirebaseKey = !string.IsNullOrEmpty(firebaseKeyJson);
            diagnostics.Add($"🔑 FIREBASE_SERVICE_ACCOUNT_JSON موجود: {hasFirebaseKey}");

            if (hasFirebaseKey)
            {
                diagnostics.Add($"🔑 طول المفتاح: {firebaseKeyJson!.Length} حرف");
                diagnostics.Add($"🔑 يبدأ بـ: {firebaseKeyJson[..50]}...");
            }
            else
            {
                return Ok(new {
                    success = false,
                    diagnostics,
                    error = "❌ متغير البيئة FIREBASE_SERVICE_ACCOUNT_JSON غير موجود!"
                });
            }

            // 3. محاولة الإرسال
            diagnostics.Add("📤 جاري الإرسال عبر NotificationService...");

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId ?? "test-user",
                Title = request.Title ?? "اختبار",
                Message = request.Message ?? "هذا إشعار تجريبي",
                Type = NotificationType.Info,
                Priority = NotificationPriority.High,
                CreatedAt = DateTimeOffset.UtcNow,
                Channels = new List<ChannelDelivery>
                {
                    new() { Channel = NotificationChannel.Firebase }
                }
            };

            var result = await _notificationService.SendAsync(notification);

            diagnostics.Add($"📊 نتيجة الإرسال: Success={result.Success}");
            diagnostics.Add($"📊 القنوات الناجحة: {string.Join(", ", result.DeliveredChannels ?? [])}");
            diagnostics.Add($"📊 القنوات الفاشلة: {string.Join(", ", result.FailedChannels ?? [])}");

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                diagnostics.Add($"❌ رسالة الخطأ: {result.ErrorMessage}");
            }

            return Ok(new
            {
                success = result.Success,
                diagnostics,
                result = new
                {
                    result.Success,
                    result.NotificationId,
                    result.ErrorMessage,
                    DeliveredChannels = result.DeliveredChannels?.ToList(),
                    FailedChannels = result.FailedChannels?.ToList(),
                    result.Metadata
                }
            });
        }
        catch (Exception ex)
        {
            diagnostics.Add($"❌ استثناء: {ex.GetType().Name}");
            diagnostics.Add($"❌ رسالة: {ex.Message}");

            if (ex.InnerException != null)
            {
                diagnostics.Add($"❌ استثناء داخلي: {ex.InnerException.Message}");
            }

            return Ok(new
            {
                success = false,
                diagnostics,
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// 🔥 اختبار إرسال Firebase مباشرة (تجاوز NotificationService لتشخيص دقيق)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("test-firebase-direct")]
    public async Task<IActionResult> TestFirebaseDirectSend([FromBody] TestFirebaseRequest request)
    {
        var diagnostics = new List<string>();

        try
        {
            diagnostics.Add("🚀 بدء الاختبار المباشر...");

            // 1. التحقق من وجود FirebaseMessagingService
            if (_firebaseMessagingService == null)
            {
                diagnostics.Add("❌ FirebaseMessagingService غير مسجل في DI Container");
                return Ok(new { success = false, diagnostics, error = "Firebase غير مُعد" });
            }
            diagnostics.Add("✅ FirebaseMessagingService موجود");

            // 2. جلب التوكن
            string token;
            string userId;
            if (string.IsNullOrEmpty(request.Token))
            {
                var firstToken = await _dbContext.Set<DeviceTokenEntity>()
                    .Where(d => d.IsActive && !d.IsDeleted)
                    .FirstOrDefaultAsync();

                if (firstToken == null)
                {
                    diagnostics.Add("❌ لا توجد توكنات في قاعدة البيانات");
                    return Ok(new { success = false, diagnostics, error = "لا توجد توكنات" });
                }

                token = firstToken.Token;
                userId = firstToken.UserId;
                diagnostics.Add($"📱 تم جلب توكن: {token[..15]}...{token[^10..]}");
                diagnostics.Add($"👤 المستخدم: {userId}");
            }
            else
            {
                token = request.Token;
                userId = request.UserId ?? "test-user";
                diagnostics.Add($"📱 توكن من الطلب: {token[..15]}...{token[^10..]}");
            }

            // 3. التحقق من إعدادات Firebase
            var firebaseKeyJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");
            diagnostics.Add($"🔑 FIREBASE_SERVICE_ACCOUNT_JSON: {(string.IsNullOrEmpty(firebaseKeyJson) ? "غير موجود" : $"{firebaseKeyJson.Length} حرف")}");

            // 4. بناء الرسالة البسيطة
            var title = request.Title ?? "اختبار مباشر";
            var body = request.Message ?? $"إشعار تجريبي مباشر - {DateTime.UtcNow:HH:mm:ss}";

            diagnostics.Add($"📝 العنوان: {title}");
            diagnostics.Add($"📝 النص: {body}");

            var message = new MulticastMessage
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Tokens = new List<string> { token }
            };

            // 5. إرسال مباشر عبر FirebaseMessagingService
            diagnostics.Add("📤 جاري الإرسال المباشر عبر Firebase SDK...");

            try
            {
                var response = await _firebaseMessagingService.SendMulticastAsync(
                    new[] { token },
                    message);

                diagnostics.Add($"✅ استجابة Firebase: Success={response.SuccessCount}, Failure={response.FailureCount}");

                // تفاصيل كل استجابة
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var r = response.Responses[i];
                    if (r.IsSuccess)
                    {
                        diagnostics.Add($"✅ Response[{i}]: نجاح - MessageId={r.MessageId}");
                    }
                    else
                    {
                        var errorMsg = r.Exception?.Message ?? "غير معروف";
                        var errorCode = (r.Exception as FirebaseMessagingException)?.MessagingErrorCode.ToString() ?? "N/A";
                        diagnostics.Add($"❌ Response[{i}]: فشل - ErrorCode={errorCode}, Message={errorMsg}");
                    }
                }

                return Ok(new
                {
                    success = response.SuccessCount > 0,
                    diagnostics,
                    response = new
                    {
                        successCount = response.SuccessCount,
                        failureCount = response.FailureCount,
                        responses = response.Responses.Select((r, i) => new
                        {
                            index = i,
                            isSuccess = r.IsSuccess,
                            messageId = r.MessageId,
                            error = r.Exception?.Message,
                            errorCode = (r.Exception as FirebaseMessagingException)?.MessagingErrorCode.ToString()
                        })
                    }
                });
            }
            catch (FirebaseMessagingException fex)
            {
                diagnostics.Add($"❌ Firebase Exception: {fex.MessagingErrorCode}");
                diagnostics.Add($"❌ Message: {fex.Message}");
                diagnostics.Add($"❌ HttpResponse: {fex.HttpResponse?.StatusCode}");

                return Ok(new
                {
                    success = false,
                    diagnostics,
                    error = fex.Message,
                    errorCode = fex.MessagingErrorCode.ToString(),
                    httpStatus = fex.HttpResponse?.StatusCode.ToString()
                });
            }
        }
        catch (Exception ex)
        {
            diagnostics.Add($"❌ استثناء عام: {ex.GetType().Name}");
            diagnostics.Add($"❌ رسالة: {ex.Message}");

            if (ex.InnerException != null)
            {
                diagnostics.Add($"❌ استثناء داخلي: {ex.InnerException.GetType().Name}");
                diagnostics.Add($"❌ رسالة داخلية: {ex.InnerException.Message}");
            }

            return Ok(new
            {
                success = false,
                diagnostics,
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                exceptionType = ex.GetType().FullName,
                stackTrace = ex.StackTrace?[..Math.Min(500, ex.StackTrace?.Length ?? 0)]
            });
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
    public DateTime CreatedAt { get; set; }
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
    public int TotalActiveDevices { get; set; }
}

public class TestFirebaseRequest
{
    public string? Token { get; set; }
    public string? UserId { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
}
