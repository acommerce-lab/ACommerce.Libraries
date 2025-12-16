using ACommerce.Marketing.MetaConversions.Models;

namespace ACommerce.Marketing.MetaConversions.Services;

public interface IMetaConversionsService
{
    Task<bool> TrackPurchaseAsync(PurchaseEventRequest request);
    
    Task<bool> TrackViewContentAsync(ViewContentEventRequest request);
    
    Task<bool> TrackSearchAsync(SearchEventRequest request);
    
    Task<bool> TrackRegistrationAsync(RegistrationEventRequest request);
    
    Task<bool> TrackLoginAsync(LoginEventRequest request);
    
    Task<bool> TrackAddToWishlistAsync(AddToWishlistEventRequest request);
    
    Task<bool> TrackCustomEventAsync(CustomEventRequest request);
    
    Task<ConversionResponse?> SendEventAsync(ConversionEvent conversionEvent);
}

public record UserContext(
    string? UserId = null,
    string? Email = null,
    string? Phone = null,
    string? FirstName = null,
    string? LastName = null,
    string? City = null,
    string? Country = null,
    string? IpAddress = null,
    string? UserAgent = null,
    string? Fbc = null,
    string? Fbp = null
);

public record PurchaseEventRequest(
    string TransactionId,
    decimal Value,
    string Currency = "SAR",
    string? ContentName = null,
    string[]? ContentIds = null,
    int NumItems = 1,
    UserContext? User = null
);

public record ViewContentEventRequest(
    string ContentId,
    string ContentName,
    string? ContentType = "listing",
    string? Category = null,
    decimal? Value = null,
    UserContext? User = null
);

public record SearchEventRequest(
    string SearchQuery,
    int? ResultsCount = null,
    UserContext? User = null
);

public record RegistrationEventRequest(
    string Method,
    UserContext? User = null
);

public record LoginEventRequest(
    string Method,
    UserContext? User = null
);

public record AddToWishlistEventRequest(
    string ContentId,
    string ContentName,
    string? ContentType = "listing",
    decimal? Value = null,
    UserContext? User = null
);

public record CustomEventRequest(
    string EventName,
    Dictionary<string, object>? Parameters = null,
    UserContext? User = null
);
