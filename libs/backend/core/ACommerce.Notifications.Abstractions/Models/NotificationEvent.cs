using ACommerce.Notifications.Abstractions.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Notifications.Abstractions.Models;

/// <summary>
/// ??? ????? ??? ???? ?? Microservices
/// </summary>
public record NotificationEvent
{
	public Guid EventId { get; init; } = Guid.NewGuid();
	public required string UserId { get; init; }
	public required string Title { get; init; }
	public required string Message { get; init; }
	public NotificationType Type { get; init; } = NotificationType.Info;
	public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
	public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
	public DateTimeOffset? ScheduledAt { get; init; }
    [NotMapped] public Dictionary<string, string>? Data { get; init; }
	public List<ChannelConfiguration> Channels { get; init; } = new();
	public string? ActionUrl { get; init; }
	public string? ImageUrl { get; init; }
	public string? Sound { get; init; }

	/// <summary>
	/// ????? Event ??? Notification
	/// </summary>
	public Notification ToNotification()
	{
		return new Notification
		{
			Id = EventId,
			UserId = UserId,
			Title = Title,
			Message = Message,
			Type = Type,
			Priority = Priority,
			CreatedAt = CreatedAt,
			ScheduledAt = ScheduledAt,
			Data = Data,
			ActionUrl = ActionUrl,
			ImageUrl = ImageUrl,
			Sound = Sound,
			Channels = [.. Channels.Select(c => new ChannelDelivery
			{
				Channel = c.Channel,
				MaxRetries = c.MaxRetries,
				RetryDelay = c.RetryDelay
			})]
		};
	}
}

/// <summary>
/// ????? ???? ???????
/// </summary>
public record ChannelConfiguration
{
	public required NotificationChannel Channel { get; init; }
	public int MaxRetries { get; init; } = 3;
	public TimeSpan RetryDelay { get; init; } = TimeSpan.FromMinutes(5);
}

