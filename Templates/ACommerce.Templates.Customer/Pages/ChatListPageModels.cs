using ACommerce.Templates.Customer.Components;

namespace ACommerce.Templates.Customer.Pages;

public class ConversationItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? LastMessage { get; set; }
    public ChatMessageType LastMessageType { get; set; }
    public DateTime LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsOnline { get; set; }
    public ConversationType Type { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? OrderId { get; set; }
}

public enum ConversationType
{
    Vendor,
    Support,
    Order
}
