using ACommerce.Marketing.Analytics.Entities;
using ACommerce.Marketing.MetaConversions.Services;
using Microsoft.Extensions.Logging;

namespace ACommerce.Marketing.Analytics.Services;

/// <summary>
/// تنفيذ خدمة تتبع الأحداث التسويقية
/// تخزن الأحداث محلياً وترسلها إلى Meta CAPI
/// </summary>
public class MarketingEventTracker : IMarketingEventTracker, IMarketingEventProcessor
{
    private readonly IAttributionService _attributionService;
    private readonly IMetaConversionsService _metaService;
    private readonly ILogger<MarketingEventTracker> _logger;

    public MarketingEventTracker(
        IAttributionService attributionService,
        IMetaConversionsService metaService,
        ILogger<MarketingEventTracker> logger)
    {
        _attributionService = attributionService;
        _metaService = metaService;
        _logger = logger;
    }

    /// <summary>
    /// معالجة حدث من الطابور
    /// </summary>
    public async Task ProcessEventAsync(MarketingQueueItem item, CancellationToken cancellationToken = default)
    {
        switch (item.EventType)
        {
            case MarketingEventType.Registration:
                await TrackRegistrationAsync((RegistrationTrackingRequest)item.Request);
                break;
            case MarketingEventType.Login:
                await TrackLoginAsync((LoginTrackingRequest)item.Request);
                break;
            case MarketingEventType.Purchase:
                await TrackPurchaseAsync((PurchaseTrackingRequest)item.Request);
                break;
            case MarketingEventType.ContentView:
                await TrackViewContentAsync((ViewContentTrackingRequest)item.Request);
                break;
            case MarketingEventType.Search:
                await TrackSearchAsync((SearchTrackingRequest)item.Request);
                break;
            case MarketingEventType.AddToWishlist:
                await TrackAddToWishlistAsync((WishlistTrackingRequest)item.Request);
                break;
            case MarketingEventType.InitiateCheckout:
                await TrackInitiateCheckoutAsync((CheckoutTrackingRequest)item.Request);
                break;
            case MarketingEventType.Lead:
                await TrackLeadAsync((LeadTrackingRequest)item.Request);
                break;
            case MarketingEventType.PageView:
                await TrackCustomEventAsync((CustomTrackingRequest)item.Request);
                break;
            default:
                _logger.LogWarning("[Marketing] Unknown event type: {EventType}", item.EventType);
                break;
        }
    }

    public async Task TrackRegistrationAsync(RegistrationTrackingRequest request)
    {
        _logger.LogInformation("[Marketing] Tracking registration: {Method}, User: {UserId}",
            request.Method, request.User.UserId);

        // 1. تخزين محلي
        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.Registration,
            Metadata = new Dictionary<string, object> { ["method"] = request.Method }
        });

        // 2. إرسال إلى Meta CAPI
        try
        {
            await _metaService.TrackRegistrationAsync(new RegistrationEventRequest(
                Method: request.Method,
                User: MapToUserContext(request.User)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Marketing] Failed to send registration to Meta CAPI");
        }
    }

    public async Task TrackLoginAsync(LoginTrackingRequest request)
    {
        _logger.LogInformation("[Marketing] Tracking login: {Method}, User: {UserId}",
            request.Method, request.User.UserId);

        // 1. تخزين محلي
        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.Login,
            Metadata = new Dictionary<string, object> { ["method"] = request.Method }
        });

        // 2. إرسال إلى Meta CAPI
        try
        {
            await _metaService.TrackLoginAsync(new LoginEventRequest(
                Method: request.Method,
                User: MapToUserContext(request.User)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Marketing] Failed to send login to Meta CAPI");
        }
    }

    public async Task TrackPurchaseAsync(PurchaseTrackingRequest request)
    {
        _logger.LogInformation("[Marketing] Tracking purchase: {TransactionId}, Value: {Value} {Currency}",
            request.TransactionId, request.Value, request.Currency);

        // 1. تخزين محلي
        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.Purchase,
            EntityId = request.TransactionId,
            EntityType = request.ContentType,
            Value = request.Value,
            Currency = request.Currency,
            Metadata = new Dictionary<string, object>
            {
                ["content_name"] = request.ContentName ?? "",
                ["content_ids"] = request.ContentIds ?? Array.Empty<string>(),
                ["num_items"] = request.NumItems
            }
        });

        // 2. إرسال إلى Meta CAPI
        try
        {
            await _metaService.TrackPurchaseAsync(new PurchaseEventRequest(
                TransactionId: request.TransactionId,
                Value: request.Value,
                Currency: request.Currency,
                ContentName: request.ContentName,
                ContentIds: request.ContentIds,
                NumItems: request.NumItems,
                User: MapToUserContext(request.User)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Marketing] Failed to send purchase to Meta CAPI");
        }
    }

    public async Task TrackViewContentAsync(ViewContentTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Tracking view content: {ContentId}", request.ContentId);

        // 1. تخزين محلي
        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.ContentView,
            EntityId = request.ContentId,
            EntityType = request.ContentType,
            Value = request.Value,
            Metadata = new Dictionary<string, object>
            {
                ["content_name"] = request.ContentName,
                ["category"] = request.Category ?? ""
            }
        });

        // 2. إرسال إلى Meta CAPI
        try
        {
            await _metaService.TrackViewContentAsync(new ViewContentEventRequest(
                ContentId: request.ContentId,
                ContentName: request.ContentName,
                ContentType: request.ContentType,
                Category: request.Category,
                Value: request.Value,
                User: MapToUserContext(request.User)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Marketing] Failed to send view content to Meta CAPI");
        }
    }

    public async Task TrackSearchAsync(SearchTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Tracking search: {Query}", request.SearchQuery);

        // 1. تخزين محلي
        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.Search,
            Metadata = new Dictionary<string, object>
            {
                ["search_query"] = request.SearchQuery,
                ["results_count"] = request.ResultsCount ?? 0
            }
        });

        // 2. إرسال إلى Meta CAPI
        try
        {
            await _metaService.TrackSearchAsync(new SearchEventRequest(
                SearchQuery: request.SearchQuery,
                ResultsCount: request.ResultsCount,
                User: MapToUserContext(request.User)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Marketing] Failed to send search to Meta CAPI");
        }
    }

    public async Task TrackAddToWishlistAsync(WishlistTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Tracking add to wishlist: {ContentId}", request.ContentId);

        // 1. تخزين محلي
        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.AddToWishlist,
            EntityId = request.ContentId,
            EntityType = request.ContentType,
            Value = request.Value,
            Metadata = new Dictionary<string, object>
            {
                ["content_name"] = request.ContentName
            }
        });

        // 2. إرسال إلى Meta CAPI
        try
        {
            await _metaService.TrackAddToWishlistAsync(new AddToWishlistEventRequest(
                ContentId: request.ContentId,
                ContentName: request.ContentName,
                ContentType: request.ContentType,
                Value: request.Value,
                User: MapToUserContext(request.User)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Marketing] Failed to send wishlist to Meta CAPI");
        }
    }

    public async Task TrackInitiateCheckoutAsync(CheckoutTrackingRequest request)
    {
        _logger.LogInformation("[Marketing] Tracking initiate checkout: {ContentId}, Value: {Value}",
            request.ContentId, request.Value);

        // 1. تخزين محلي
        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.InitiateCheckout,
            EntityId = request.ContentId,
            EntityType = "listing",
            Value = request.Value,
            Currency = request.Currency,
            Metadata = new Dictionary<string, object>
            {
                ["content_name"] = request.ContentName,
                ["num_items"] = request.NumItems
            }
        });

        // 2. إرسال إلى Meta CAPI (حدث مخصص)
        try
        {
            await _metaService.TrackCustomEventAsync(new CustomEventRequest(
                EventName: "InitiateCheckout",
                Parameters: new Dictionary<string, object>
                {
                    ["content_ids"] = new[] { request.ContentId },
                    ["content_name"] = request.ContentName,
                    ["value"] = request.Value,
                    ["currency"] = request.Currency,
                    ["num_items"] = request.NumItems
                },
                User: MapToUserContext(request.User)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Marketing] Failed to send checkout to Meta CAPI");
        }
    }

    public async Task TrackLeadAsync(LeadTrackingRequest request)
    {
        _logger.LogInformation("[Marketing] Tracking lead: {Type}, Content: {ContentId}",
            request.LeadType, request.ContentId);

        // 1. تخزين محلي
        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.Lead,
            EntityId = request.ContentId,
            EntityType = "listing",
            Value = request.Value,
            Metadata = new Dictionary<string, object>
            {
                ["lead_type"] = request.LeadType,
                ["content_name"] = request.ContentName ?? ""
            }
        });

        // 2. إرسال إلى Meta CAPI (حدث مخصص)
        try
        {
            await _metaService.TrackCustomEventAsync(new CustomEventRequest(
                EventName: "Lead",
                Parameters: new Dictionary<string, object>
                {
                    ["content_id"] = request.ContentId ?? "",
                    ["content_name"] = request.ContentName ?? "",
                    ["lead_type"] = request.LeadType,
                    ["value"] = request.Value ?? 0
                },
                User: MapToUserContext(request.User)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Marketing] Failed to send lead to Meta CAPI");
        }
    }

    public async Task TrackCustomEventAsync(CustomTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Tracking custom event: {EventName}", request.EventName);

        // 1. تخزين محلي (نستخدم ContentView كنوع عام)
        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.PageView,
            Metadata = request.Parameters
        });

        // 2. إرسال إلى Meta CAPI
        try
        {
            await _metaService.TrackCustomEventAsync(new CustomEventRequest(
                EventName: request.EventName,
                Parameters: request.Parameters,
                User: MapToUserContext(request.User)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Marketing] Failed to send custom event to Meta CAPI");
        }
    }

    private UserContext MapToUserContext(UserTrackingContext user)
    {
        return new UserContext(
            UserId: user.UserId,
            Email: user.Email,
            Phone: user.Phone,
            FirstName: user.FirstName,
            LastName: user.LastName,
            City: user.City,
            Country: user.Country ?? "sa",
            IpAddress: user.IpAddress,
            UserAgent: user.UserAgent,
            Fbc: user.Fbc,
            Fbp: user.Fbp
        );
    }
}
