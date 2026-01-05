using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Marketing.Analytics.Entities;

public class MarketingAttribution : IBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public string? UserId { get; set; }
    public string? SessionId { get; set; }

    public MarketingPlatform Platform { get; set; }
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

public enum MarketingPlatform
{
    Direct = 0,
    Organic = 1,
    Meta = 2,
    Google = 3,
    TikTok = 4,
    Snapchat = 5,
    Twitter = 6,
    Email = 7,
    SMS = 8,
    Referral = 9,
    Other = 99
}
