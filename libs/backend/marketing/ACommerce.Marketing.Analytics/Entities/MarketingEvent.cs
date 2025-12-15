namespace ACommerce.Marketing.Analytics.Entities;

public class MarketingEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? AttributionId { get; set; }
    public MarketingAttribution? Attribution { get; set; }
    
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    
    public MarketingEventType EventType { get; set; }
    public string? EntityId { get; set; }
    public string? EntityType { get; set; }
    
    public decimal? Value { get; set; }
    public string? Currency { get; set; }
    
    public string? Metadata { get; set; }
}

public enum MarketingEventType
{
    PageView = 1,
    ContentView = 2,
    Search = 3,
    AddToCart = 4,
    AddToWishlist = 5,
    InitiateCheckout = 6,
    Purchase = 7,
    Registration = 8,
    Login = 9,
    Share = 10,
    Lead = 11,
    Contact = 12
}
