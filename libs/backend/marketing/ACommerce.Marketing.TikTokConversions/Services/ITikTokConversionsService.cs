using ACommerce.Marketing.TikTokConversions.Models;

namespace ACommerce.Marketing.TikTokConversions.Services;

public interface ITikTokConversionsService
{
    Task<bool> TrackPurchaseAsync(TikTokPurchaseEventRequest request);
    Task<bool> TrackViewContentAsync(TikTokViewContentEventRequest request);
    Task<bool> TrackSearchAsync(TikTokSearchEventRequest request);
    Task<bool> TrackRegistrationAsync(TikTokRegistrationEventRequest request);
    Task<bool> TrackLoginAsync(TikTokLoginEventRequest request);
    Task<bool> TrackAddToWishlistAsync(TikTokAddToWishlistEventRequest request);
    Task<bool> TrackCustomEventAsync(TikTokCustomEventRequest request);
}
