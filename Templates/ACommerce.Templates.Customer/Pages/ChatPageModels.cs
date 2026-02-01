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

// Chat list models for order-based vendor communication
public class ChatListInfo
{
    public string Id { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string? VendorLogo { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsVendorOnline { get; set; }
}

public class ChatInfo
{
    public string Id { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string? VendorLogo { get; set; }
    public bool IsVendorOnline { get; set; }
}

public class ChatMessageInfo
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsFromMe { get; set; }
    public bool IsRead { get; set; }
    public string Type { get; set; } = "text";
    public string? MediaUrl { get; set; }
}
