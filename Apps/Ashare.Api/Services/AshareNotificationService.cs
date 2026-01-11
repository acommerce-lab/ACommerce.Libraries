using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Ashare.Api.Services;

/// <summary>
/// خدمة الإشعارات الخاصة بتطبيق عشير
/// تبسط إرسال الإشعارات للأحداث المختلفة
/// </summary>
public interface IAshareNotificationService
{
    /// <summary>
    /// إرسال إشعار رسالة جديدة
    /// </summary>
    Task SendNewMessageNotificationAsync(string recipientUserId, string senderName, string messagePreview, Guid chatId);

    /// <summary>
    /// إرسال إشعار حجز جديد للمضيف
    /// </summary>
    Task SendNewBookingNotificationAsync(string hostUserId, string spaceName, string customerName, Guid bookingId);

    /// <summary>
    /// إرسال إشعار تأكيد الحجز للعميل
    /// </summary>
    Task SendBookingConfirmedNotificationAsync(string customerUserId, string spaceName, Guid bookingId);

    /// <summary>
    /// إرسال إشعار رفض الحجز للعميل
    /// </summary>
    Task SendBookingRejectedNotificationAsync(string customerUserId, string spaceName, string reason, Guid bookingId);

    /// <summary>
    /// إرسال إشعار إلغاء الحجز
    /// </summary>
    Task SendBookingCancelledNotificationAsync(string userId, string spaceName, Guid bookingId, bool isHost);
}

/// <summary>
/// تنفيذ خدمة الإشعارات
/// </summary>
public class AshareNotificationService : IAshareNotificationService
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<AshareNotificationService> _logger;

    public AshareNotificationService(
        INotificationService notificationService,
        ILogger<AshareNotificationService> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task SendNewMessageNotificationAsync(
        string recipientUserId,
        string senderName,
        string messagePreview,
        Guid chatId)
    {
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = recipientUserId,
                Title = $"رسالة جديدة من {senderName}",
                Message = messagePreview.Length > 100 ? messagePreview[..100] + "..." : messagePreview,
                Type = NotificationType.ChatMessage,
                Priority = NotificationPriority.High,
                CreatedAt = DateTimeOffset.UtcNow,
                ActionUrl = $"/chat/{chatId}",
                Sound = "default",
                Channels = new List<ChannelDelivery>
                {
                    new() { Channel = NotificationChannel.InApp },
                    new() { Channel = NotificationChannel.Firebase }
                },
                Data = new Dictionary<string, string>
                {
                    ["type"] = "new_message",
                    ["chatId"] = chatId.ToString(),
                    ["senderName"] = senderName
                }
            };

            var result = await _notificationService.SendAsync(notification);
            _logger.LogInformation("New message notification sent to {UserId}, success: {Success}",
                recipientUserId, result.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send new message notification to {UserId}", recipientUserId);
        }
    }

    public async Task SendNewBookingNotificationAsync(
        string hostUserId,
        string spaceName,
        string customerName,
        Guid bookingId)
    {
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = hostUserId,
                Title = "طلب حجز جديد 🏠",
                Message = $"لديك طلب حجز جديد على {spaceName} من {customerName}",
                Type = NotificationType.NewBooking,
                Priority = NotificationPriority.High,
                CreatedAt = DateTimeOffset.UtcNow,
                ActionUrl = $"/booking/{bookingId}",
                Sound = "default",
                BadgeCount = 1,
                Channels = new List<ChannelDelivery>
                {
                    new() { Channel = NotificationChannel.InApp },
                    new() { Channel = NotificationChannel.Firebase }
                },
                Data = new Dictionary<string, string>
                {
                    ["type"] = "new_booking",
                    ["bookingId"] = bookingId.ToString(),
                    ["spaceName"] = spaceName
                }
            };

            var result = await _notificationService.SendAsync(notification);
            _logger.LogInformation("New booking notification sent to host {UserId}, success: {Success}",
                hostUserId, result.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send new booking notification to host {UserId}", hostUserId);
        }
    }

    public async Task SendBookingConfirmedNotificationAsync(
        string customerUserId,
        string spaceName,
        Guid bookingId)
    {
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = customerUserId,
                Title = "تم تأكيد حجزك ✅",
                Message = $"تم تأكيد حجزك على {spaceName}. استمتع بإقامتك!",
                Type = NotificationType.BookingUpdate,
                Priority = NotificationPriority.High,
                CreatedAt = DateTimeOffset.UtcNow,
                ActionUrl = $"/booking/{bookingId}",
                Sound = "default",
                Channels = new List<ChannelDelivery>
                {
                    new() { Channel = NotificationChannel.InApp },
                    new() { Channel = NotificationChannel.Firebase }
                },
                Data = new Dictionary<string, string>
                {
                    ["type"] = "booking_confirmed",
                    ["bookingId"] = bookingId.ToString(),
                    ["spaceName"] = spaceName
                }
            };

            var result = await _notificationService.SendAsync(notification);
            _logger.LogInformation("Booking confirmed notification sent to {UserId}, success: {Success}",
                customerUserId, result.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking confirmed notification to {UserId}", customerUserId);
        }
    }

    public async Task SendBookingRejectedNotificationAsync(
        string customerUserId,
        string spaceName,
        string reason,
        Guid bookingId)
    {
        try
        {
            var message = string.IsNullOrEmpty(reason)
                ? $"للأسف، تم رفض طلب حجزك على {spaceName}"
                : $"للأسف، تم رفض طلب حجزك على {spaceName}. السبب: {reason}";

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = customerUserId,
                Title = "تم رفض الحجز ❌",
                Message = message,
                Type = NotificationType.BookingUpdate,
                Priority = NotificationPriority.High,
                CreatedAt = DateTimeOffset.UtcNow,
                ActionUrl = $"/booking/{bookingId}",
                Sound = "default",
                Channels = new List<ChannelDelivery>
                {
                    new() { Channel = NotificationChannel.InApp },
                    new() { Channel = NotificationChannel.Firebase }
                },
                Data = new Dictionary<string, string>
                {
                    ["type"] = "booking_rejected",
                    ["bookingId"] = bookingId.ToString(),
                    ["spaceName"] = spaceName,
                    ["reason"] = reason ?? ""
                }
            };

            var result = await _notificationService.SendAsync(notification);
            _logger.LogInformation("Booking rejected notification sent to {UserId}, success: {Success}",
                customerUserId, result.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking rejected notification to {UserId}", customerUserId);
        }
    }

    public async Task SendBookingCancelledNotificationAsync(
        string userId,
        string spaceName,
        Guid bookingId,
        bool isHost)
    {
        try
        {
            var title = isHost ? "تم إلغاء الحجز" : "تم إلغاء حجزك";
            var message = isHost
                ? $"قام العميل بإلغاء حجزه على {spaceName}"
                : $"تم إلغاء حجزك على {spaceName}";

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationType.BookingUpdate,
                Priority = NotificationPriority.Normal,
                CreatedAt = DateTimeOffset.UtcNow,
                ActionUrl = $"/booking/{bookingId}",
                Channels = new List<ChannelDelivery>
                {
                    new() { Channel = NotificationChannel.InApp },
                    new() { Channel = NotificationChannel.Firebase }
                },
                Data = new Dictionary<string, string>
                {
                    ["type"] = "booking_cancelled",
                    ["bookingId"] = bookingId.ToString(),
                    ["spaceName"] = spaceName
                }
            };

            var result = await _notificationService.SendAsync(notification);
            _logger.LogInformation("Booking cancelled notification sent to {UserId}, success: {Success}",
                userId, result.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking cancelled notification to {UserId}", userId);
        }
    }
}
