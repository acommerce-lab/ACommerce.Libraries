using ACommerce.Notifications.Abstractions.Enums;

namespace ACommerce.Notifications.Messaging.Commands;

/// <summary>
/// Command to send a notification via messaging bus
/// </summary>
public record SendNotificationCommand
{
    public required string UserId { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public NotificationType Type { get; init; } = NotificationType.Info;
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
    public List<NotificationChannel> Channels { get; init; } = [NotificationChannel.Email];
    public Dictionary<string, object>? Data { get; init; }
}