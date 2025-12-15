namespace Ashare.Admin.Services;

public class MarketingAnalyticsService
{
    public async Task<MarketingAnalyticsData> GetAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        return new MarketingAnalyticsData
        {
            PlatformStats = new List<PlatformStatsDto>
            {
                new() { Platform = "Meta", PlatformIcon = "📘", Visits = 4520, Registrations = 245, Purchases = 89, Revenue = 45000m, ConversionRate = 1.97m },
                new() { Platform = "Google", PlatformIcon = "🔍", Visits = 3200, Registrations = 178, Purchases = 67, Revenue = 32500m, ConversionRate = 2.09m },
                new() { Platform = "TikTok", PlatformIcon = "🎵", Visits = 2100, Registrations = 156, Purchases = 45, Revenue = 18500m, ConversionRate = 2.14m },
                new() { Platform = "Snapchat", PlatformIcon = "👻", Visits = 1450, Registrations = 89, Purchases = 28, Revenue = 12000m, ConversionRate = 1.93m },
                new() { Platform = "Direct", PlatformIcon = "🔗", Visits = 980, Registrations = 45, Purchases = 23, Revenue = 9800m, ConversionRate = 2.35m },
                new() { Platform = "Organic", PlatformIcon = "🌱", Visits = 750, Registrations = 34, Purchases = 18, Revenue = 7200m, ConversionRate = 2.40m },
            },
            DailyStats = GenerateDailyStats(startDate, endDate),
            TopCampaigns = new List<CampaignStatsDto>
            {
                new() { Campaign = "ramadan_2025", Platform = "Meta", Visits = 1250, Registrations = 89, Purchases = 34, Revenue = 17500m },
                new() { Campaign = "riyadh_launch", Platform = "Google", Visits = 980, Registrations = 67, Purchases = 28, Revenue = 14200m },
                new() { Campaign = "summer_promo", Platform = "TikTok", Visits = 870, Registrations = 78, Purchases = 25, Revenue = 11500m },
                new() { Campaign = "jeddah_special", Platform = "Meta", Visits = 650, Registrations = 45, Purchases = 19, Revenue = 9800m },
                new() { Campaign = "workspace_deal", Platform = "Snapchat", Visits = 520, Registrations = 38, Purchases = 15, Revenue = 7500m },
            },
            Totals = new MarketingTotalsDto
            {
                TotalVisits = 13000,
                TotalRegistrations = 747,
                TotalPurchases = 270,
                TotalRevenue = 125000m,
                ConversionRate = 2.08m
            }
        };
    }

    private List<DailyStatsDto> GenerateDailyStats(DateTime startDate, DateTime endDate)
    {
        var random = new Random(42);
        var stats = new List<DailyStatsDto>();
        
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            stats.Add(new DailyStatsDto
            {
                Date = date,
                Visits = random.Next(300, 600),
                Registrations = random.Next(15, 40),
                Purchases = random.Next(5, 20),
                Revenue = random.Next(2000, 8000)
            });
        }
        
        return stats;
    }
}

public class MarketingAnalyticsData
{
    public List<PlatformStatsDto> PlatformStats { get; set; } = new();
    public List<DailyStatsDto> DailyStats { get; set; } = new();
    public List<CampaignStatsDto> TopCampaigns { get; set; } = new();
    public MarketingTotalsDto Totals { get; set; } = new();
}

public class PlatformStatsDto
{
    public string Platform { get; set; } = string.Empty;
    public string PlatformIcon { get; set; } = string.Empty;
    public int Visits { get; set; }
    public int Registrations { get; set; }
    public int Purchases { get; set; }
    public decimal Revenue { get; set; }
    public decimal ConversionRate { get; set; }
}

public class DailyStatsDto
{
    public DateTime Date { get; set; }
    public int Visits { get; set; }
    public int Registrations { get; set; }
    public int Purchases { get; set; }
    public decimal Revenue { get; set; }
}

public class CampaignStatsDto
{
    public string Campaign { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public int Visits { get; set; }
    public int Registrations { get; set; }
    public int Purchases { get; set; }
    public decimal Revenue { get; set; }
}

public class MarketingTotalsDto
{
    public int TotalVisits { get; set; }
    public int TotalRegistrations { get; set; }
    public int TotalPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal ConversionRate { get; set; }
}
