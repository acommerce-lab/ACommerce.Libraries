using ACommerce.Marketing.Analytics.Entities;

namespace ACommerce.Marketing.Analytics.Services;

/// <summary>
/// خدمة موحدة لتتبع الأحداث التسويقية
/// تخزن الأحداث محلياً وترسلها إلى Meta CAPI
/// </summary>
public interface IMarketingEventTracker
{
    /// <summary>
    /// تتبع حدث التسجيل
    /// </summary>
    Task TrackRegistrationAsync(RegistrationTrackingRequest request);

    /// <summary>
    /// تتبع حدث تسجيل الدخول
    /// </summary>
    Task TrackLoginAsync(LoginTrackingRequest request);

    /// <summary>
    /// تتبع حدث الشراء/الحجز
    /// </summary>
    Task TrackPurchaseAsync(PurchaseTrackingRequest request);

    /// <summary>
    /// تتبع حدث مشاهدة المحتوى
    /// </summary>
    Task TrackViewContentAsync(ViewContentTrackingRequest request);

    /// <summary>
    /// تتبع حدث البحث
    /// </summary>
    Task TrackSearchAsync(SearchTrackingRequest request);

    /// <summary>
    /// تتبع حدث الإضافة للمفضلة
    /// </summary>
    Task TrackAddToWishlistAsync(WishlistTrackingRequest request);

    /// <summary>
    /// تتبع حدث بدء الحجز (InitiateCheckout)
    /// </summary>
    Task TrackInitiateCheckoutAsync(CheckoutTrackingRequest request);

    /// <summary>
    /// تتبع حدث التواصل/الاستفسار (Lead)
    /// </summary>
    Task TrackLeadAsync(LeadTrackingRequest request);

    /// <summary>
    /// تتبع حدث مخصص
    /// </summary>
    Task TrackCustomEventAsync(CustomTrackingRequest request);
}

#region Tracking Request Models

/// <summary>
/// بيانات المستخدم المشتركة للتتبع
/// </summary>
public class UserTrackingContext
{
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "sa";
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    /// <summary>
    /// Facebook Click ID (من URL parameter fbclid)
    /// </summary>
    public string? Fbc { get; set; }
    /// <summary>
    /// Facebook Browser ID (من cookie _fbp)
    /// </summary>
    public string? Fbp { get; set; }
    /// <summary>
    /// Google Click ID (من URL parameter gclid)
    /// </summary>
    public string? Gclid { get; set; }
    /// <summary>
    /// TikTok Click ID (من URL parameter ttclid)
    /// </summary>
    public string? Ttclid { get; set; }
    /// <summary>
    /// TikTok Pixel ID (من cookie _ttp)
    /// </summary>
    public string? Ttp { get; set; }
    /// <summary>
    /// Snapchat Click ID (من URL parameter ScCid)
    /// </summary>
    public string? ScClickId { get; set; }
    /// <summary>
    /// Twitter/X Click ID (من URL parameter twclid)
    /// </summary>
    public string? Twclid { get; set; }
}

public class RegistrationTrackingRequest
{
    public string Method { get; set; } = "nafath";
    public UserTrackingContext User { get; set; } = new();
}

public class LoginTrackingRequest
{
    public string Method { get; set; } = "nafath";
    public UserTrackingContext User { get; set; } = new();
}

public class PurchaseTrackingRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? ContentName { get; set; }
    public string[]? ContentIds { get; set; }
    public string? ContentType { get; set; } = "booking";
    public int NumItems { get; set; } = 1;
    public UserTrackingContext User { get; set; } = new();
}

public class ViewContentTrackingRequest
{
    public string ContentId { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public string? ContentType { get; set; } = "listing";
    public string? Category { get; set; }
    public decimal? Value { get; set; }
    public UserTrackingContext User { get; set; } = new();
}

public class SearchTrackingRequest
{
    public string SearchQuery { get; set; } = string.Empty;
    public int? ResultsCount { get; set; }
    public UserTrackingContext User { get; set; } = new();
}

public class WishlistTrackingRequest
{
    public string ContentId { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public string? ContentType { get; set; } = "listing";
    public decimal? Value { get; set; }
    public UserTrackingContext User { get; set; } = new();
}

public class CheckoutTrackingRequest
{
    public string ContentId { get; set; } = string.Empty;
    public string ContentName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Currency { get; set; } = "SAR";
    public int NumItems { get; set; } = 1;
    public UserTrackingContext User { get; set; } = new();
}

public class LeadTrackingRequest
{
    public string? ContentId { get; set; }
    public string? ContentName { get; set; }
    public string LeadType { get; set; } = "inquiry";
    public decimal? Value { get; set; }
    public UserTrackingContext User { get; set; } = new();
}

public class CustomTrackingRequest
{
    public string EventName { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
    public UserTrackingContext User { get; set; } = new();
}

#endregion
