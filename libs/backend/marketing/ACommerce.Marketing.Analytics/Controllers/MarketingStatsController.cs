using ACommerce.Marketing.Analytics.Entities;
using ACommerce.Marketing.Analytics.Queries;
using ACommerce.Marketing.Analytics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Marketing.Analytics.Controllers;

/// <summary>
/// متحكم إحصائيات التسويق
/// </summary>
[ApiController]
[Route("api/marketing")]
[Authorize(Roles = "Admin")]
public class MarketingStatsController : ControllerBase
{
    private readonly IMarketingStatsService _statsService;

    public MarketingStatsController(IMarketingStatsService statsService)
    {
        _statsService = statsService;
    }

    /// <summary>
    /// الحصول على إحصائيات التسويق
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<MarketingStatsResponse>> GetStats(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] MarketingPlatform? platform = null)
    {
        var query = new GetMarketingStatsQuery
        {
            StartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            EndDate = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Platform = platform
        };

        var result = await _statsService.GetStatsAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// الحصول على إحصائيات المنصات
    /// </summary>
    [HttpGet("platforms")]
    public async Task<ActionResult<List<PlatformStats>>> GetPlatformStats(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await _statsService.GetPlatformStatsAsync(start, end);
        return Ok(result);
    }

    /// <summary>
    /// الحصول على أفضل الحملات
    /// </summary>
    [HttpGet("campaigns")]
    public async Task<ActionResult<List<CampaignStats>>> GetTopCampaigns(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] int limit = 10)
    {
        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await _statsService.GetTopCampaignsAsync(start, end, limit);
        return Ok(result);
    }
}
