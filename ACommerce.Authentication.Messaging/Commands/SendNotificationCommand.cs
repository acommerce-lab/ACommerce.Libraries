using ACommerce.Notifications.Abstractions.Enums;

namespace ACommerce.Authentication.Messaging.Commands;

public record SendNotificationCommand
{
    public required string UserId { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public NotificationType Type { get; init; }
    public NotificationPriority Priority { get; init; }
    public List<NotificationChannel> Channels { get; init; } = [];
    public Dictionary<string, object>? Data { get; init; }
}