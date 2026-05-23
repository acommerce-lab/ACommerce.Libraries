namespace ACommerce.Templates.Customer.Pages;

public class NotificationItem
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.General;
    public DateTime CreatedAt { get; set; }

    // Compatibility property used by templates expecting Timestamp
    public DateTime Timestamp => CreatedAt;
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}

public enum NotificationType
{
    General,
    Order,
    Promotion,
    Delivery,
    Payment,
    Review,
    Message,
    Account
}
