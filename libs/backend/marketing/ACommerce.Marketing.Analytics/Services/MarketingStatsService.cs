using ACommerce.Marketing.Analytics.Entities;
using ACommerce.Marketing.Analytics.Queries;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Marketing.Analytics.Services;

public class MarketingStatsService : IMarketingStatsService
{
    private readonly DbContext _dbContext;

    public MarketingStatsService(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MarketingStatsResponse> GetStatsAsync(GetMarketingStatsQuery query)
    {
        var statsQuery = _dbContext.Set<DailyMarketingStats>()
            .Where(s => s.Date >= query.StartDate && s.Date <= query.EndDate);

        if (query.Platform.HasValue)
        {
            statsQuery = statsQuery.Where(s => s.Platform == query.Platform.Value);
        }

        var allStats = await statsQuery.ToListAsync();

        var byPlatform = allStats
            .GroupBy(s => s.Platform)
            .Select(g => new PlatformStats
            {
                Platform = g.Key,
                Visits = g.Sum(s => s.Visits),
                Registrations = g.Sum(s => s.Registrations),
                Purchases = g.Sum(s => s.Purchases),
                Revenue = g.Sum(s => s.Revenue)
            })
            .OrderByDescending(p => p.Revenue)
            .ToList();

        var byDay = allStats
            .GroupBy(s => s.Date)
            .Select(g => new DailyStats
            {
                Date = g.Key,
                Visits = g.Sum(s => s.Visits),
                Registrations = g.Sum(s => s.Registrations),
                Purchases = g.Sum(s => s.Purchases),
                Revenue = g.Sum(s => s.Revenue)
            })
            .OrderBy(d => d.Date)
            .ToList();

        var topCampaigns = allStats
            .Where(s => !string.IsNullOrEmpty(s.Campaign))
            .GroupBy(s => new { s.Campaign, s.Platform })
            .Select(g => new CampaignStats
            {
                Campaign = g.Key.Campaign,
                Platform = g.Key.Platform,
                Visits = g.Sum(s => s.Visits),
                Registrations = g.Sum(s => s.Registrations),
                Purchases = g.Sum(s => s.Purchases),
                Revenue = g.Sum(s => s.Revenue)
            })
            .OrderByDescending(c => c.Revenue)
            .Take(10)
            .ToList();

        return new MarketingStatsResponse
        {
            ByPlatform = byPlatform,
            ByDay = byDay,
            TopCampaigns = topCampaigns,
            Totals = new MarketingTotals
            {
                TotalVisits = allStats.Sum(s => s.Visits),
                TotalRegistrations = allStats.Sum(s => s.Registrations),
                TotalPurchases = allStats.Sum(s => s.Purchases),
                TotalRevenue = allStats.Sum(s => s.Revenue)
            }
        };
    }

    public async Task<List<PlatformStats>> GetPlatformStatsAsync(DateOnly startDate, DateOnly endDate)
    {
        var query = new GetMarketingStatsQuery { StartDate = startDate, EndDate = endDate };
        var response = await GetStatsAsync(query);
        return response.ByPlatform;
    }

    public async Task<List<CampaignStats>> GetTopCampaignsAsync(DateOnly startDate, DateOnly endDate, int limit = 10)
    {
        var stats = await _dbContext.Set<DailyMarketingStats>()
            .Where(s => s.Date >= startDate && s.Date <= endDate && !string.IsNullOrEmpty(s.Campaign))
            .GroupBy(s => new { s.Campaign, s.Platform })
            .Select(g => new CampaignStats
            {
                Campaign = g.Key.Campaign,
                Platform = g.Key.Platform,
                Visits = g.Sum(s => s.Visits),
                Registrations = g.Sum(s => s.Registrations),
                Purchases = g.Sum(s => s.Purchases),
                Revenue = g.Sum(s => s.Revenue)
            })
            .OrderByDescending(c => c.Revenue)
            .Take(limit)
            .ToListAsync();

        return stats;
    }
}
