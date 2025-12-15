using System.Text.Json;
using ACommerce.Marketing.Analytics.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Marketing.Analytics.Services;

public class AttributionService : IAttributionService
{
    private readonly DbContext _dbContext;

    public AttributionService(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MarketingAttribution> CaptureAttributionAsync(AttributionCaptureRequest request)
    {
        var platform = DetectPlatform(request);
        
        var attribution = new MarketingAttribution
        {
            SessionId = request.SessionId,
            UserId = request.UserId,
            Platform = platform,
            UtmSource = request.UtmSource,
            UtmMedium = request.UtmMedium,
            UtmCampaign = request.UtmCampaign,
            UtmContent = request.UtmContent,
            UtmTerm = request.UtmTerm,
            ClickId = request.ClickId,
            ReferrerUrl = request.ReferrerUrl,
            LandingPage = request.LandingPage,
            DeviceType = request.DeviceType,
            Country = request.Country,
            City = request.City
        };

        _dbContext.Set<MarketingAttribution>().Add(attribution);
        await _dbContext.SaveChangesAsync();
        
        return attribution;
    }

    public async Task<MarketingAttribution?> GetAttributionBySessionAsync(string sessionId)
    {
        return await _dbContext.Set<MarketingAttribution>()
            .Where(a => a.SessionId == sessionId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<MarketingAttribution?> GetAttributionByUserAsync(string userId)
    {
        return await _dbContext.Set<MarketingAttribution>()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AssociateUserWithAttributionAsync(Guid attributionId, string userId)
    {
        var attribution = await _dbContext.Set<MarketingAttribution>()
            .FirstOrDefaultAsync(a => a.Id == attributionId);
            
        if (attribution != null && string.IsNullOrEmpty(attribution.UserId))
        {
            attribution.UserId = userId;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task TrackEventAsync(MarketingEventRequest request)
    {
        MarketingAttribution? attribution = null;
        
        if (!string.IsNullOrEmpty(request.UserId))
        {
            attribution = await GetAttributionByUserAsync(request.UserId);
        }
        else if (!string.IsNullOrEmpty(request.SessionId))
        {
            attribution = await GetAttributionBySessionAsync(request.SessionId);
        }

        var marketingEvent = new MarketingEvent
        {
            AttributionId = attribution?.Id,
            UserId = request.UserId,
            SessionId = request.SessionId,
            EventType = request.EventType,
            EntityId = request.EntityId,
            EntityType = request.EntityType,
            Value = request.Value,
            Currency = request.Currency,
            Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null
        };

        _dbContext.Set<MarketingEvent>().Add(marketingEvent);
        await _dbContext.SaveChangesAsync();
        
        await UpdateDailyStatsAsync(attribution?.Platform ?? MarketingPlatform.Direct, 
                                    attribution?.UtmCampaign, 
                                    request.EventType, 
                                    request.Value);
    }

    private async Task UpdateDailyStatsAsync(MarketingPlatform platform, string? campaign, 
                                              MarketingEventType eventType, decimal? value)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var stats = await _dbContext.Set<DailyMarketingStats>()
            .FirstOrDefaultAsync(s => s.Date == today && s.Platform == platform && s.Campaign == campaign);
            
        if (stats == null)
        {
            stats = new DailyMarketingStats
            {
                Date = today,
                Platform = platform,
                Campaign = campaign
            };
            _dbContext.Set<DailyMarketingStats>().Add(stats);
        }

        switch (eventType)
        {
            case MarketingEventType.PageView:
                stats.PageViews++;
                break;
            case MarketingEventType.ContentView:
                stats.ContentViews++;
                break;
            case MarketingEventType.Search:
                stats.Searches++;
                break;
            case MarketingEventType.AddToCart:
                stats.AddToCart++;
                break;
            case MarketingEventType.AddToWishlist:
                stats.AddToWishlist++;
                break;
            case MarketingEventType.Registration:
                stats.Registrations++;
                break;
            case MarketingEventType.Purchase:
                stats.Purchases++;
                stats.Revenue += value ?? 0;
                break;
        }
        
        stats.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    private MarketingPlatform DetectPlatform(AttributionCaptureRequest request)
    {
        if (!string.IsNullOrEmpty(request.ClickId))
        {
            if (request.ClickId.StartsWith("fb") || request.UtmSource?.ToLower() == "facebook" || request.UtmSource?.ToLower() == "instagram")
                return MarketingPlatform.Meta;
            if (request.ClickId.StartsWith("gclid") || request.UtmSource?.ToLower() == "google")
                return MarketingPlatform.Google;
            if (request.ClickId.StartsWith("tt") || request.UtmSource?.ToLower() == "tiktok")
                return MarketingPlatform.TikTok;
            if (request.ClickId.StartsWith("sc") || request.UtmSource?.ToLower() == "snapchat")
                return MarketingPlatform.Snapchat;
        }

        if (!string.IsNullOrEmpty(request.UtmSource))
        {
            return request.UtmSource.ToLower() switch
            {
                "facebook" or "instagram" or "meta" => MarketingPlatform.Meta,
                "google" or "adwords" => MarketingPlatform.Google,
                "tiktok" => MarketingPlatform.TikTok,
                "snapchat" => MarketingPlatform.Snapchat,
                "twitter" or "x" => MarketingPlatform.Twitter,
                "email" or "newsletter" => MarketingPlatform.Email,
                "sms" => MarketingPlatform.SMS,
                _ => MarketingPlatform.Other
            };
        }

        if (!string.IsNullOrEmpty(request.ReferrerUrl))
        {
            var referrer = request.ReferrerUrl.ToLower();
            if (referrer.Contains("google")) return MarketingPlatform.Organic;
            if (referrer.Contains("facebook") || referrer.Contains("instagram")) return MarketingPlatform.Meta;
            if (referrer.Contains("tiktok")) return MarketingPlatform.TikTok;
            if (referrer.Contains("snapchat")) return MarketingPlatform.Snapchat;
            if (referrer.Contains("twitter") || referrer.Contains("x.com")) return MarketingPlatform.Twitter;
            return MarketingPlatform.Referral;
        }

        return MarketingPlatform.Direct;
    }
}
