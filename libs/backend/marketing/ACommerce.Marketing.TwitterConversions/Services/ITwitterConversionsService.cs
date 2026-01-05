using ACommerce.Marketing.TwitterConversions.Models;

namespace ACommerce.Marketing.TwitterConversions.Services;

public interface ITwitterConversionsService
{
    Task<bool> TrackPurchaseAsync(TwitterPurchaseEventRequest request);
    Task<bool> TrackViewContentAsync(TwitterViewContentEventRequest request);
    Task<bool> TrackSearchAsync(TwitterSearchEventRequest request);
    Task<bool> TrackRegistrationAsync(TwitterRegistrationEventRequest request);
    Task<bool> TrackLoginAsync(TwitterLoginEventRequest request);
    Task<bool> TrackAddToWishlistAsync(TwitterAddToWishlistEventRequest request);
    Task<bool> TrackCustomEventAsync(TwitterCustomEventRequest request);
}
