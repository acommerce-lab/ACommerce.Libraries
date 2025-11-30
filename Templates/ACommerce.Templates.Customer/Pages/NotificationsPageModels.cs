namespace ACommerce.Templates.Customer.Pages;

public class NotificationItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime Timestamp { get; set; }
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
