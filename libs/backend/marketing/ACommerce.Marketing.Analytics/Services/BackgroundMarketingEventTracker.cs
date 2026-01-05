using ACommerce.Marketing.Analytics.Entities;
using Microsoft.Extensions.Logging;

namespace ACommerce.Marketing.Analytics.Services;

/// <summary>
/// تنفيذ خدمة تتبع الأحداث التسويقية باستخدام الطابور الخلفي
/// تضع الأحداث في الطابور وتعود فوراً بدون انتظار
/// </summary>
public class BackgroundMarketingEventTracker : IMarketingEventTracker
{
    private readonly IMarketingEventQueue _queue;
    private readonly ILogger<BackgroundMarketingEventTracker> _logger;

    public BackgroundMarketingEventTracker(
        IMarketingEventQueue queue,
        ILogger<BackgroundMarketingEventTracker> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    public Task TrackRegistrationAsync(RegistrationTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Queuing registration event for User: {UserId}", request.User.UserId);
        _queue.Enqueue(new MarketingQueueItem
        {
            EventType = MarketingEventType.Registration,
            Request = request
        });
        return Task.CompletedTask;
    }

    public Task TrackLoginAsync(LoginTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Queuing login event for User: {UserId}", request.User.UserId);
        _queue.Enqueue(new MarketingQueueItem
        {
            EventType = MarketingEventType.Login,
            Request = request
        });
        return Task.CompletedTask;
    }

    public Task TrackPurchaseAsync(PurchaseTrackingRequest request)
    {
        _logger.LogInformation("📊 [Marketing] Queuing purchase event: {TransactionId}, Value: {Value} {Currency}",
            request.TransactionId, request.Value, request.Currency);

        var item = new MarketingQueueItem
        {
            EventType = MarketingEventType.Purchase,
            Request = request
        };
        _queue.Enqueue(item);

        _logger.LogInformation("✅ [Marketing] Purchase event queued successfully at {Time}", item.EnqueuedAt);
        return Task.CompletedTask;
    }

    public Task TrackViewContentAsync(ViewContentTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Queuing view content event: {ContentId}", request.ContentId);
        _queue.Enqueue(new MarketingQueueItem
        {
            EventType = MarketingEventType.ContentView,
            Request = request
        });
        return Task.CompletedTask;
    }

    public Task TrackSearchAsync(SearchTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Queuing search event: {Query}", request.SearchQuery);
        _queue.Enqueue(new MarketingQueueItem
        {
            EventType = MarketingEventType.Search,
            Request = request
        });
        return Task.CompletedTask;
    }

    public Task TrackAddToWishlistAsync(WishlistTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Queuing wishlist event: {ContentId}", request.ContentId);
        _queue.Enqueue(new MarketingQueueItem
        {
            EventType = MarketingEventType.AddToWishlist,
            Request = request
        });
        return Task.CompletedTask;
    }

    public Task TrackInitiateCheckoutAsync(CheckoutTrackingRequest request)
    {
        _logger.LogInformation("[Marketing] Queuing checkout event: {ContentId}, Value: {Value}",
            request.ContentId, request.Value);
        _queue.Enqueue(new MarketingQueueItem
        {
            EventType = MarketingEventType.InitiateCheckout,
            Request = request
        });
        return Task.CompletedTask;
    }

    public Task TrackLeadAsync(LeadTrackingRequest request)
    {
        _logger.LogInformation("[Marketing] Queuing lead event: {Type}", request.LeadType);
        _queue.Enqueue(new MarketingQueueItem
        {
            EventType = MarketingEventType.Lead,
            Request = request
        });
        return Task.CompletedTask;
    }

    public Task TrackCustomEventAsync(CustomTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Queuing custom event: {EventName}", request.EventName);
        _queue.Enqueue(new MarketingQueueItem
        {
            EventType = MarketingEventType.PageView, // نستخدم PageView للأحداث المخصصة
            Request = request
        });
        return Task.CompletedTask;
    }
}
