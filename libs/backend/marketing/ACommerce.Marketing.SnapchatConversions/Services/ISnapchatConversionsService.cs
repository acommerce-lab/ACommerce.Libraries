using ACommerce.Marketing.SnapchatConversions.Models;

namespace ACommerce.Marketing.SnapchatConversions.Services;

public interface ISnapchatConversionsService
{
    Task<bool> TrackPurchaseAsync(SnapchatPurchaseEventRequest request);
    Task<bool> TrackViewContentAsync(SnapchatViewContentEventRequest request);
    Task<bool> TrackSearchAsync(SnapchatSearchEventRequest request);
    Task<bool> TrackRegistrationAsync(SnapchatRegistrationEventRequest request);
    Task<bool> TrackLoginAsync(SnapchatLoginEventRequest request);
    Task<bool> TrackAddToWishlistAsync(SnapchatAddToWishlistEventRequest request);
    Task<bool> TrackCustomEventAsync(SnapchatCustomEventRequest request);
}
