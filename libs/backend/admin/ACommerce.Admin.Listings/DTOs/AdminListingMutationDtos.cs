using ACommerce.Catalog.Listings.Enums;

namespace ACommerce.Admin.Listings.DTOs;

/// <summary>
/// طلب تغيير حالة عرض (موافقة / رفض / تعليق …).
/// </summary>
public class ChangeListingStatusRequest
{
    /// <summary>سبب التغيير — إلزامي عند الرفض، اختياري لغيره.</summary>
    public string? Reason { get; set; }
}

/// <summary>
/// طلب تفعيل/تعطيل عرض.
/// </summary>
public class SetListingActiveRequest
{
    public bool IsActive { get; set; }
}

/// <summary>
/// نتيجة عملية إدارية على عرض — تُستخدم لكتابة سجل التدقيق.
/// </summary>
public class AdminListingMutationResult
{
    public bool Success { get; set; }
    public Guid ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public ListingStatus OldStatus { get; set; }
    public ListingStatus NewStatus { get; set; }
    public bool OldIsActive { get; set; }
    public bool NewIsActive { get; set; }
    public string? Message { get; set; }

    public static AdminListingMutationResult NotFound(Guid id) => new()
    {
        Success = false,
        ListingId = id,
        Message = "Listing not found"
    };
}
