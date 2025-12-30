using ACommerce.Marketing.GoogleConversions.Models;

namespace ACommerce.Marketing.GoogleConversions.Services;

public interface IGoogleConversionsService
{
    Task<bool> TrackPurchaseAsync(GooglePurchaseEventRequest request);
    Task<bool> TrackViewContentAsync(GoogleViewContentEventRequest request);
    Task<bool> TrackSearchAsync(GoogleSearchEventRequest request);
    Task<bool> TrackRegistrationAsync(GoogleRegistrationEventRequest request);
    Task<bool> TrackLoginAsync(GoogleLoginEventRequest request);
    Task<bool> TrackAddToWishlistAsync(GoogleAddToWishlistEventRequest request);
    Task<bool> TrackCustomEventAsync(GoogleCustomEventRequest request);
}
