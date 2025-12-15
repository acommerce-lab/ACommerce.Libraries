using ACommerce.Marketing.Analytics.Entities;

namespace ACommerce.Marketing.Analytics.Services;

public interface IAttributionService
{
    Task<MarketingAttribution> CaptureAttributionAsync(AttributionCaptureRequest request);
    Task<MarketingAttribution?> GetAttributionBySessionAsync(string sessionId);
    Task<MarketingAttribution?> GetAttributionByUserAsync(string userId);
    Task AssociateUserWithAttributionAsync(Guid attributionId, string userId);
    Task TrackEventAsync(MarketingEventRequest request);
}

public class AttributionCaptureRequest
{
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmTerm { get; set; }
    public string? ClickId { get; set; }
    public string? ReferrerUrl { get; set; }
    public string? LandingPage { get; set; }
    public string? DeviceType { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}

public class MarketingEventRequest
{
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public MarketingEventType EventType { get; set; }
    public string? EntityId { get; set; }
    public string? EntityType { get; set; }
    public decimal? Value { get; set; }
    public string? Currency { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
