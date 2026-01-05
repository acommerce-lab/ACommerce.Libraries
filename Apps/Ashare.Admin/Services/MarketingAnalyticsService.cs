using System.Net.Http.Json;
using ACommerce.Marketing.Analytics.Entities;

namespace Ashare.Admin.Services;

public class MarketingAnalyticsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public MarketingAnalyticsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        var baseUrl = _configuration["ApiBaseUrl"] ?? "https://api.ashare.sa";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<MarketingAnalyticsData> GetAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = DateOnly.FromDateTime(startDate);
            var end = DateOnly.FromDateTime(endDate);

            var response = await _httpClient.GetFromJsonAsync<MarketingStatsApiResponse>(
                $"/api/marketing/stats?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}");

            if (response != null)
            {
                return MapToAnalyticsData(response);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Marketing] Error fetching stats: {ex.Message}");
        }

        // إرجاع بيانات فارغة في حالة الخطأ
        return new MarketingAnalyticsData
        {
            PlatformStats = new List<PlatformStatsDto>(),
            DailyStats = new List<DailyStatsDto>(),
            TopCampaigns = new List<CampaignStatsDto>(),
            Totals = new MarketingTotalsDto()
        };
    }

    private MarketingAnalyticsData MapToAnalyticsData(MarketingStatsApiResponse response)
    {
        return new MarketingAnalyticsData
        {
            PlatformStats = response.ByPlatform?.Select(p => new PlatformStatsDto
            {
                Platform = GetPlatformName(p.Platform),
                PlatformIcon = GetPlatformIcon(p.Platform),
                Visits = p.Visits,
                Registrations = p.Registrations,
                Purchases = p.Purchases,
                Revenue = p.Revenue,
                ConversionRate = p.Visits > 0 ? (decimal)p.Purchases / p.Visits * 100 : 0
            }).ToList() ?? new List<PlatformStatsDto>(),

            DailyStats = response.ByDay?.Select(d => new DailyStatsDto
            {
                Date = d.Date.ToDateTime(TimeOnly.MinValue),
                Visits = d.Visits,
                Registrations = d.Registrations,
                Purchases = d.Purchases,
                Revenue = d.Revenue
            }).ToList() ?? new List<DailyStatsDto>(),

            TopCampaigns = response.TopCampaigns?.Select(c => new CampaignStatsDto
            {
                Campaign = c.Campaign ?? "بدون حملة",
                Platform = GetPlatformName(c.Platform),
                Visits = c.Visits,
                Registrations = c.Registrations,
                Purchases = c.Purchases,
                Revenue = c.Revenue
            }).ToList() ?? new List<CampaignStatsDto>(),

            Totals = new MarketingTotalsDto
            {
                TotalVisits = response.Totals?.TotalVisits ?? 0,
                TotalRegistrations = response.Totals?.TotalRegistrations ?? 0,
                TotalPurchases = response.Totals?.TotalPurchases ?? 0,
                TotalRevenue = response.Totals?.TotalRevenue ?? 0,
                ConversionRate = response.Totals?.TotalVisits > 0
                    ? (decimal)(response.Totals?.TotalPurchases ?? 0) / response.Totals.TotalVisits * 100
                    : 0
            }
        };
    }

    private string GetPlatformName(MarketingPlatform platform) => platform switch
    {
        MarketingPlatform.Meta => "Meta",
        MarketingPlatform.Google => "Google",
        MarketingPlatform.TikTok => "TikTok",
        MarketingPlatform.Snapchat => "Snapchat",
        MarketingPlatform.Twitter => "Twitter",
        MarketingPlatform.Email => "Email",
        MarketingPlatform.SMS => "SMS",
        MarketingPlatform.Referral => "Referral",
        MarketingPlatform.Organic => "Organic",
        MarketingPlatform.Direct => "Direct",
        _ => "Other"
    };

    private string GetPlatformIcon(MarketingPlatform platform) => platform switch
    {
        MarketingPlatform.Meta => "📘",
        MarketingPlatform.Google => "🔍",
        MarketingPlatform.TikTok => "🎵",
        MarketingPlatform.Snapchat => "👻",
        MarketingPlatform.Twitter => "🐦",
        MarketingPlatform.Email => "📧",
        MarketingPlatform.SMS => "💬",
        MarketingPlatform.Referral => "🔗",
        MarketingPlatform.Organic => "🌱",
        MarketingPlatform.Direct => "🎯",
        _ => "📊"
    };
}

// API Response Models
public class MarketingStatsApiResponse
{
    public List<PlatformStatsApi>? ByPlatform { get; set; }
    public List<DailyStatsApi>? ByDay { get; set; }
    public List<CampaignStatsApi>? TopCampaigns { get; set; }
    public TotalsApi? Totals { get; set; }
}

public class PlatformStatsApi
{
    public MarketingPlatform Platform { get; set; }
    public int Visits { get; set; }
    public int Registrations { get; set; }
    public int Purchases { get; set; }
    public decimal Revenue { get; set; }
}

public class DailyStatsApi
{
    public DateOnly Date { get; set; }
    public int Visits { get; set; }
    public int Registrations { get; set; }
    public int Purchases { get; set; }
    public decimal Revenue { get; set; }
}

public class CampaignStatsApi
{
    public string? Campaign { get; set; }
    public MarketingPlatform Platform { get; set; }
    public int Visits { get; set; }
    public int Registrations { get; set; }
    public int Purchases { get; set; }
    public decimal Revenue { get; set; }
}

public class TotalsApi
{
    public int TotalVisits { get; set; }
    public int TotalRegistrations { get; set; }
    public int TotalPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
}

// Admin DTOs
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
