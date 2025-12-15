using ACommerce.Marketing.Analytics.Entities;
using ACommerce.Marketing.Analytics.Queries;

namespace ACommerce.Marketing.Analytics.Services;

public interface IMarketingStatsService
{
    Task<MarketingStatsResponse> GetStatsAsync(GetMarketingStatsQuery query);
    Task<List<PlatformStats>> GetPlatformStatsAsync(DateOnly startDate, DateOnly endDate);
    Task<List<CampaignStats>> GetTopCampaignsAsync(DateOnly startDate, DateOnly endDate, int limit = 10);
}
