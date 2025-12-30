using ACommerce.Marketing.Analytics.Entities;
using ACommerce.Marketing.MetaConversions.Services;
using ACommerce.Marketing.GoogleConversions.Services;
using ACommerce.Marketing.GoogleConversions.Models;
using ACommerce.Marketing.TikTokConversions.Services;
using ACommerce.Marketing.TikTokConversions.Models;
using ACommerce.Marketing.SnapchatConversions.Services;
using ACommerce.Marketing.SnapchatConversions.Models;
using ACommerce.Marketing.TwitterConversions.Services;
using ACommerce.Marketing.TwitterConversions.Models;
using Microsoft.Extensions.Logging;

namespace ACommerce.Marketing.Analytics.Services;

/// <summary>
/// تنفيذ خدمة تتبع الأحداث التسويقية
/// تخزن الأحداث محلياً وترسلها إلى جميع منصات الإعلانات
/// </summary>
public class MarketingEventTracker : IMarketingEventTracker, IMarketingEventProcessor
{
    private readonly IAttributionService _attributionService;
    private readonly IMetaConversionsService _metaService;
    private readonly IGoogleConversionsService _googleService;
    private readonly ITikTokConversionsService _tiktokService;
    private readonly ISnapchatConversionsService _snapchatService;
    private readonly ITwitterConversionsService _twitterService;
    private readonly ILogger<MarketingEventTracker> _logger;

    public MarketingEventTracker(
        IAttributionService attributionService,
        IMetaConversionsService metaService,
        IGoogleConversionsService googleService,
        ITikTokConversionsService tiktokService,
        ISnapchatConversionsService snapchatService,
        ITwitterConversionsService twitterService,
        ILogger<MarketingEventTracker> logger)
    {
        _attributionService = attributionService;
        _metaService = metaService;
        _googleService = googleService;
        _tiktokService = tiktokService;
        _snapchatService = snapchatService;
        _twitterService = twitterService;
        _logger = logger;
    }

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

        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.Registration,
            Metadata = new Dictionary<string, object> { ["method"] = request.Method }
        });

        await SendToAllPlatformsAsync(
            () => _metaService.TrackRegistrationAsync(new RegistrationEventRequest(
                Method: request.Method,
                User: MapToMetaUserContext(request.User)
            )),
            () => _googleService.TrackRegistrationAsync(new GoogleRegistrationEventRequest
            {
                Method = request.Method,
                User = MapToGoogleUserContext(request.User)
            }),
            () => _tiktokService.TrackRegistrationAsync(new TikTokRegistrationEventRequest
            {
                User = MapToTikTokUserContext(request.User)
            }),
            () => _snapchatService.TrackRegistrationAsync(new SnapchatRegistrationEventRequest
            {
                Method = request.Method,
                User = MapToSnapchatUserContext(request.User)
            }),
            () => _twitterService.TrackRegistrationAsync(new TwitterRegistrationEventRequest
            {
                User = MapToTwitterUserContext(request.User)
            }),
            "registration"
        );
    }

    public async Task TrackLoginAsync(LoginTrackingRequest request)
    {
        _logger.LogInformation("[Marketing] Tracking login: {Method}, User: {UserId}",
            request.Method, request.User.UserId);

        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.Login,
            Metadata = new Dictionary<string, object> { ["method"] = request.Method }
        });

        await SendToAllPlatformsAsync(
            () => _metaService.TrackLoginAsync(new LoginEventRequest(
                Method: request.Method,
                User: MapToMetaUserContext(request.User)
            )),
            () => _googleService.TrackLoginAsync(new GoogleLoginEventRequest
            {
                Method = request.Method,
                User = MapToGoogleUserContext(request.User)
            }),
            () => _tiktokService.TrackLoginAsync(new TikTokLoginEventRequest
            {
                User = MapToTikTokUserContext(request.User)
            }),
            () => _snapchatService.TrackLoginAsync(new SnapchatLoginEventRequest
            {
                User = MapToSnapchatUserContext(request.User)
            }),
            () => _twitterService.TrackLoginAsync(new TwitterLoginEventRequest
            {
                User = MapToTwitterUserContext(request.User)
            }),
            "login"
        );
    }

    public async Task TrackPurchaseAsync(PurchaseTrackingRequest request)
    {
        _logger.LogInformation("[Marketing] Tracking purchase: {TransactionId}, Value: {Value} {Currency}",
            request.TransactionId, request.Value, request.Currency);

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

        await SendToAllPlatformsAsync(
            () => _metaService.TrackPurchaseAsync(new PurchaseEventRequest(
                TransactionId: request.TransactionId,
                Value: request.Value,
                Currency: request.Currency,
                ContentName: request.ContentName,
                ContentIds: request.ContentIds,
                NumItems: request.NumItems,
                User: MapToMetaUserContext(request.User)
            )),
            () => _googleService.TrackPurchaseAsync(new GooglePurchaseEventRequest
            {
                TransactionId = request.TransactionId,
                Value = request.Value,
                Currency = request.Currency,
                User = MapToGoogleUserContext(request.User),
                Items = request.ContentIds?.Select(id => new GoogleItem
                {
                    ItemId = id,
                    ItemName = request.ContentName,
                    Price = request.Value / Math.Max(request.NumItems, 1),
                    Quantity = 1
                }).ToList()
            }),
            () => _tiktokService.TrackPurchaseAsync(new TikTokPurchaseEventRequest
            {
                TransactionId = request.TransactionId,
                Value = request.Value,
                Currency = request.Currency,
                User = MapToTikTokUserContext(request.User),
                Contents = request.ContentIds?.Select(id => new TikTokContent
                {
                    ContentId = id,
                    ContentName = request.ContentName,
                    Price = request.Value / Math.Max(request.NumItems, 1),
                    Quantity = 1
                }).ToList()
            }),
            () => _snapchatService.TrackPurchaseAsync(new SnapchatPurchaseEventRequest
            {
                TransactionId = request.TransactionId,
                Value = request.Value,
                Currency = request.Currency,
                User = MapToSnapchatUserContext(request.User),
                ItemIds = request.ContentIds?.ToList(),
                NumberItems = request.NumItems
            }),
            () => _twitterService.TrackPurchaseAsync(new TwitterPurchaseEventRequest
            {
                TransactionId = request.TransactionId,
                Value = request.Value,
                Currency = request.Currency,
                User = MapToTwitterUserContext(request.User),
                NumberItems = request.NumItems,
                Contents = request.ContentIds?.Select(id => new TwitterContent
                {
                    ContentId = id,
                    ContentName = request.ContentName,
                    ContentPrice = (request.Value / Math.Max(request.NumItems, 1)).ToString("F2")
                }).ToList()
            }),
            "purchase"
        );
    }

    public async Task TrackViewContentAsync(ViewContentTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Tracking view content: {ContentId}", request.ContentId);

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

        await SendToAllPlatformsAsync(
            () => _metaService.TrackViewContentAsync(new ViewContentEventRequest(
                ContentId: request.ContentId,
                ContentName: request.ContentName,
                ContentType: request.ContentType,
                Category: request.Category,
                Value: request.Value,
                User: MapToMetaUserContext(request.User)
            )),
            () => _googleService.TrackViewContentAsync(new GoogleViewContentEventRequest
            {
                ContentId = request.ContentId,
                ContentName = request.ContentName,
                ContentType = request.ContentType,
                Category = request.Category,
                Value = request.Value,
                User = MapToGoogleUserContext(request.User)
            }),
            () => _tiktokService.TrackViewContentAsync(new TikTokViewContentEventRequest
            {
                ContentId = request.ContentId,
                ContentName = request.ContentName,
                ContentType = request.ContentType,
                Category = request.Category,
                Value = request.Value,
                User = MapToTikTokUserContext(request.User)
            }),
            () => _snapchatService.TrackViewContentAsync(new SnapchatViewContentEventRequest
            {
                ContentId = request.ContentId,
                ContentName = request.ContentName,
                Category = request.Category,
                Value = request.Value,
                User = MapToSnapchatUserContext(request.User)
            }),
            () => _twitterService.TrackViewContentAsync(new TwitterViewContentEventRequest
            {
                ContentId = request.ContentId,
                ContentName = request.ContentName,
                ContentType = request.ContentType,
                Value = request.Value,
                User = MapToTwitterUserContext(request.User)
            }),
            "view_content"
        );
    }

    public async Task TrackSearchAsync(SearchTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Tracking search: {Query}", request.SearchQuery);

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

        await SendToAllPlatformsAsync(
            () => _metaService.TrackSearchAsync(new SearchEventRequest(
                SearchQuery: request.SearchQuery,
                ResultsCount: request.ResultsCount,
                User: MapToMetaUserContext(request.User)
            )),
            () => _googleService.TrackSearchAsync(new GoogleSearchEventRequest
            {
                SearchQuery = request.SearchQuery,
                User = MapToGoogleUserContext(request.User)
            }),
            () => _tiktokService.TrackSearchAsync(new TikTokSearchEventRequest
            {
                SearchQuery = request.SearchQuery,
                User = MapToTikTokUserContext(request.User)
            }),
            () => _snapchatService.TrackSearchAsync(new SnapchatSearchEventRequest
            {
                SearchQuery = request.SearchQuery,
                User = MapToSnapchatUserContext(request.User)
            }),
            () => _twitterService.TrackSearchAsync(new TwitterSearchEventRequest
            {
                SearchQuery = request.SearchQuery,
                User = MapToTwitterUserContext(request.User)
            }),
            "search"
        );
    }

    public async Task TrackAddToWishlistAsync(WishlistTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Tracking add to wishlist: {ContentId}", request.ContentId);

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

        await SendToAllPlatformsAsync(
            () => _metaService.TrackAddToWishlistAsync(new AddToWishlistEventRequest(
                ContentId: request.ContentId,
                ContentName: request.ContentName,
                ContentType: request.ContentType,
                Value: request.Value,
                User: MapToMetaUserContext(request.User)
            )),
            () => _googleService.TrackAddToWishlistAsync(new GoogleAddToWishlistEventRequest
            {
                ContentId = request.ContentId,
                ContentName = request.ContentName,
                ContentType = request.ContentType,
                Value = request.Value,
                User = MapToGoogleUserContext(request.User)
            }),
            () => _tiktokService.TrackAddToWishlistAsync(new TikTokAddToWishlistEventRequest
            {
                ContentId = request.ContentId,
                ContentName = request.ContentName,
                ContentType = request.ContentType,
                Value = request.Value,
                User = MapToTikTokUserContext(request.User)
            }),
            () => _snapchatService.TrackAddToWishlistAsync(new SnapchatAddToWishlistEventRequest
            {
                ContentId = request.ContentId,
                ContentName = request.ContentName,
                Value = request.Value,
                User = MapToSnapchatUserContext(request.User)
            }),
            () => _twitterService.TrackAddToWishlistAsync(new TwitterAddToWishlistEventRequest
            {
                ContentId = request.ContentId,
                ContentName = request.ContentName,
                Value = request.Value,
                User = MapToTwitterUserContext(request.User)
            }),
            "add_to_wishlist"
        );
    }

    public async Task TrackInitiateCheckoutAsync(CheckoutTrackingRequest request)
    {
        _logger.LogInformation("[Marketing] Tracking initiate checkout: {ContentId}, Value: {Value}",
            request.ContentId, request.Value);

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

        await SendToAllPlatformsAsync(
            () => _metaService.TrackCustomEventAsync(new CustomEventRequest(
                EventName: "InitiateCheckout",
                Parameters: new Dictionary<string, object>
                {
                    ["content_ids"] = new[] { request.ContentId },
                    ["content_name"] = request.ContentName,
                    ["value"] = request.Value,
                    ["currency"] = request.Currency,
                    ["num_items"] = request.NumItems
                },
                User: MapToMetaUserContext(request.User)
            )),
            () => _googleService.TrackCustomEventAsync(new GoogleCustomEventRequest
            {
                EventName = "begin_checkout",
                User = MapToGoogleUserContext(request.User),
                Params = new Dictionary<string, object>
                {
                    ["value"] = request.Value,
                    ["currency"] = request.Currency
                }
            }),
            () => _tiktokService.TrackCustomEventAsync(new TikTokCustomEventRequest
            {
                EventName = "InitiateCheckout",
                User = MapToTikTokUserContext(request.User),
                Properties = new TikTokProperties
                {
                    Value = request.Value,
                    Currency = request.Currency
                }
            }),
            () => _snapchatService.TrackCustomEventAsync(new SnapchatCustomEventRequest
            {
                EventType = "START_CHECKOUT",
                User = MapToSnapchatUserContext(request.User)
            }),
            () => _twitterService.TrackCustomEventAsync(new TwitterCustomEventRequest
            {
                EventId = "tw-checkout",
                User = MapToTwitterUserContext(request.User)
            }),
            "initiate_checkout"
        );
    }

    public async Task TrackLeadAsync(LeadTrackingRequest request)
    {
        _logger.LogInformation("[Marketing] Tracking lead: {Type}, Content: {ContentId}",
            request.LeadType, request.ContentId);

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

        await SendToAllPlatformsAsync(
            () => _metaService.TrackCustomEventAsync(new CustomEventRequest(
                EventName: "Lead",
                Parameters: new Dictionary<string, object>
                {
                    ["content_id"] = request.ContentId ?? "",
                    ["content_name"] = request.ContentName ?? "",
                    ["lead_type"] = request.LeadType,
                    ["value"] = request.Value ?? 0
                },
                User: MapToMetaUserContext(request.User)
            )),
            () => _googleService.TrackCustomEventAsync(new GoogleCustomEventRequest
            {
                EventName = "generate_lead",
                User = MapToGoogleUserContext(request.User),
                Params = new Dictionary<string, object>
                {
                    ["value"] = request.Value ?? 0,
                    ["lead_type"] = request.LeadType
                }
            }),
            () => _tiktokService.TrackCustomEventAsync(new TikTokCustomEventRequest
            {
                EventName = "SubmitForm",
                User = MapToTikTokUserContext(request.User)
            }),
            () => _snapchatService.TrackCustomEventAsync(new SnapchatCustomEventRequest
            {
                EventType = "CUSTOM_EVENT_1",
                Description = $"Lead: {request.LeadType}",
                User = MapToSnapchatUserContext(request.User)
            }),
            () => _twitterService.TrackCustomEventAsync(new TwitterCustomEventRequest
            {
                EventId = "tw-lead",
                Description = request.LeadType,
                User = MapToTwitterUserContext(request.User)
            }),
            "lead"
        );
    }

    public async Task TrackCustomEventAsync(CustomTrackingRequest request)
    {
        _logger.LogDebug("[Marketing] Tracking custom event: {EventName}", request.EventName);

        await _attributionService.TrackEventAsync(new MarketingEventRequest
        {
            UserId = request.User.UserId,
            SessionId = request.User.SessionId,
            EventType = MarketingEventType.PageView,
            Metadata = request.Parameters
        });

        await SendToAllPlatformsAsync(
            () => _metaService.TrackCustomEventAsync(new CustomEventRequest(
                EventName: request.EventName,
                Parameters: request.Parameters,
                User: MapToMetaUserContext(request.User)
            )),
            () => _googleService.TrackCustomEventAsync(new GoogleCustomEventRequest
            {
                EventName = request.EventName,
                Params = request.Parameters,
                User = MapToGoogleUserContext(request.User)
            }),
            () => _tiktokService.TrackCustomEventAsync(new TikTokCustomEventRequest
            {
                EventName = request.EventName,
                User = MapToTikTokUserContext(request.User)
            }),
            () => _snapchatService.TrackCustomEventAsync(new SnapchatCustomEventRequest
            {
                EventType = "CUSTOM_EVENT_1",
                Description = request.EventName,
                User = MapToSnapchatUserContext(request.User)
            }),
            () => _twitterService.TrackCustomEventAsync(new TwitterCustomEventRequest
            {
                EventId = $"tw-{request.EventName.ToLower()}",
                User = MapToTwitterUserContext(request.User)
            }),
            "custom"
        );
    }

    private async Task SendToAllPlatformsAsync(
        Func<Task<bool>> metaTask,
        Func<Task<bool>> googleTask,
        Func<Task<bool>> tiktokTask,
        Func<Task<bool>> snapchatTask,
        Func<Task<bool>> twitterTask,
        string eventName)
    {
        var tasks = new List<(string Platform, Task<bool> Task)>
        {
            ("Meta", SafeExecuteAsync(metaTask)),
            ("Google", SafeExecuteAsync(googleTask)),
            ("TikTok", SafeExecuteAsync(tiktokTask)),
            ("Snapchat", SafeExecuteAsync(snapchatTask)),
            ("Twitter", SafeExecuteAsync(twitterTask))
        };

        await Task.WhenAll(tasks.Select(t => t.Task));

        foreach (var (platform, task) in tasks)
        {
            var result = await task;
            if (!result)
            {
                _logger.LogDebug("[Marketing] {Platform} {Event} - skipped or failed", platform, eventName);
            }
        }
    }

    private async Task<bool> SafeExecuteAsync(Func<Task<bool>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Marketing] Platform API call failed");
            return false;
        }
    }

    #region User Context Mappers

    private UserContext MapToMetaUserContext(UserTrackingContext user)
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

    private GoogleUserContext MapToGoogleUserContext(UserTrackingContext user)
    {
        return new GoogleUserContext
        {
            UserId = user.UserId,
            ClientId = user.SessionId,
            Email = user.Email,
            Phone = user.Phone,
            FirstName = user.FirstName,
            LastName = user.LastName,
            City = user.City,
            Country = user.Country ?? "sa",
            IpAddress = user.IpAddress,
            UserAgent = user.UserAgent,
            Gclid = user.Gclid,
            SessionId = user.SessionId
        };
    }

    private TikTokUserContext MapToTikTokUserContext(UserTrackingContext user)
    {
        return new TikTokUserContext
        {
            UserId = user.UserId,
            Email = user.Email,
            Phone = user.Phone,
            IpAddress = user.IpAddress,
            UserAgent = user.UserAgent,
            Ttclid = user.Ttclid,
            Ttp = user.Ttp
        };
    }

    private SnapchatUserContext MapToSnapchatUserContext(UserTrackingContext user)
    {
        return new SnapchatUserContext
        {
            UserId = user.UserId,
            Email = user.Email,
            Phone = user.Phone,
            IpAddress = user.IpAddress,
            UserAgent = user.UserAgent,
            ScClickId = user.ScClickId,
            UuidC1 = user.SessionId
        };
    }

    private TwitterUserContext MapToTwitterUserContext(UserTrackingContext user)
    {
        return new TwitterUserContext
        {
            UserId = user.UserId,
            Email = user.Email,
            Phone = user.Phone,
            IpAddress = user.IpAddress,
            UserAgent = user.UserAgent,
            Twclid = user.Twclid
        };
    }

    #endregion
}
