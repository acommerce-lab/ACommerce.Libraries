using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Marketing.Analytics.Entities;

public class DailyMarketingStats : IBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public DateOnly Date { get; set; }
    public MarketingPlatform Platform { get; set; }
    public string? Campaign { get; set; }

    public int Visits { get; set; }
    public int UniqueVisitors { get; set; }
    public int Registrations { get; set; }
    public int Purchases { get; set; }
    public decimal Revenue { get; set; }

    public int PageViews { get; set; }
    public int ContentViews { get; set; }
    public int Searches { get; set; }
    public int AddToCart { get; set; }
    public int AddToWishlist { get; set; }
}
