using ACommerce.Notifications.Abstractions.Enums;

namespace ACommerce.Authentication.Messaging.Options;

/// <summary>
/// Configuration options for authentication messaging
/// </summary>
public class AuthenticationMessagingOptions
{
    /// <summary>
    /// Notification channels to use for authentication events
    /// Default: Email only
    /// </summary>
    public List<NotificationChannel> NotificationChannels { get; set; } =
        [NotificationChannel.Email];

    /// <summary>
    /// Enable notifications for TwoFactorInitiated events
    /// Default: true
    /// </summary>
    public bool NotifyOnInitiation { get; set; } = true;

    /// <summary>
    /// Enable notifications for TwoFactorSucceeded events
    /// Default: true
    /// </summary>
    public bool NotifyOnSuccess { get; set; } = true;

    /// <summary>
    /// Enable notifications for TwoFactorFailed events
    /// Default: true
    /// </summary>
    public bool NotifyOnFailure { get; set; } = true;

    /// <summary>
    /// Enable notifications for TwoFactorExpired events
    /// Default: true
    /// </summary>
    public bool NotifyOnExpiration { get; set; } = true;

    /// <summary>
    /// Priority for success notifications
    /// Default: High
    /// </summary>
    public NotificationPriority SuccessPriority { get; set; } = NotificationPriority.High;

    /// <summary>
    /// Priority for failure notifications
    /// Default: Urgent
    /// </summary>
    public NotificationPriority FailurePriority { get; set; } = NotificationPriority.Urgent;

    /// <summary>
    /// Priority for initiation notifications
    /// Default: Normal
    /// </summary>
    public NotificationPriority InitiationPriority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// Priority for expiration notifications
    /// Default: Normal
    /// </summary>
    public NotificationPriority ExpirationPriority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// Custom message templates (optional)
    /// Key: EventType (e.g., "TwoFactorSucceeded")
    /// Value: Message template with {Provider}, {TransactionId}, etc.
    /// </summary>
    public Dictionary<string, string>? MessageTemplates { get; set; }

    /// <summary>
    /// Custom title templates (optional)
    /// Key: EventType
    /// Value: Title template
    /// </summary>
    public Dictionary<string, string>? TitleTemplates { get; set; }

    /// <summary>
    /// Include transaction details in notifications
    /// Default: true
    /// </summary>
    public bool IncludeTransactionDetails { get; set; } = true;
}