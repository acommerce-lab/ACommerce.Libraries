using ACommerce.Templates.Customer.Components;

namespace ACommerce.Templates.Customer.Pages;

public class ChatContactInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
}

public class ChatMessageItem
{
    public Guid Id { get; set; }
    public string? Text { get; set; }
    public ChatMessageType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsSent { get; set; }
    public bool IsDelivered { get; set; }
    public bool IsRead { get; set; }
    public string? MediaUrl { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImage { get; set; }
    public decimal? ProductPrice { get; set; }
    public Guid? ProductId { get; set; }
    public string? OrderNumber { get; set; }
}

public class RelatedOrderInfo
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
}
