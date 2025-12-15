using ACommerce.Marketing.Analytics.Entities;

namespace ACommerce.Marketing.Analytics.Queries;

public class GetMarketingStatsQuery
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public MarketingPlatform? Platform { get; set; }
}

public class MarketingStatsResponse
{
    public List<PlatformStats> ByPlatform { get; set; } = new();
    public List<DailyStats> ByDay { get; set; } = new();
    public List<CampaignStats> TopCampaigns { get; set; } = new();
    public MarketingTotals Totals { get; set; } = new();
}

public class PlatformStats
{
    public MarketingPlatform Platform { get; set; }
    public string PlatformName => Platform.ToString();
    public int Visits { get; set; }
    public int Registrations { get; set; }
    public int Purchases { get; set; }
    public decimal Revenue { get; set; }
    public decimal ConversionRate => Visits > 0 ? (decimal)Purchases / Visits * 100 : 0;
}

public class DailyStats
{
    public DateOnly Date { get; set; }
    public int Visits { get; set; }
    public int Registrations { get; set; }
    public int Purchases { get; set; }
    public decimal Revenue { get; set; }
}

public class CampaignStats
{
    public string? Campaign { get; set; }
    public MarketingPlatform Platform { get; set; }
    public int Visits { get; set; }
    public int Registrations { get; set; }
    public int Purchases { get; set; }
    public decimal Revenue { get; set; }
}

public class MarketingTotals
{
    public int TotalVisits { get; set; }
    public int TotalRegistrations { get; set; }
    public int TotalPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal OverallConversionRate => TotalVisits > 0 ? (decimal)TotalPurchases / TotalVisits * 100 : 0;
}
