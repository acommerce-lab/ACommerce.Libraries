using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.Subscriptions.Enums;

namespace ACommerce.Subscriptions.Entities;

/// <summary>
/// سجل أحداث الاشتراك - لتتبع تاريخ الاشتراك
/// </summary>
public class SubscriptionEvent : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    #region Relations - العلاقات

    /// <summary>معرف الاشتراك</summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>الاشتراك</summary>
    public Subscription? Subscription { get; set; }

    /// <summary>معرف المزود</summary>
    public Guid VendorId { get; set; }

    #endregion

    #region Event Info - معلومات الحدث

    /// <summary>نوع الحدث</summary>
    public SubscriptionEventType EventType { get; set; }

    /// <summary>وصف الحدث</summary>
    public string? Description { get; set; }

    /// <summary>تاريخ الحدث</summary>
    public DateTime EventDate { get; set; }

    #endregion

    #region State Change - تغيير الحالة

    /// <summary>الحالة السابقة</summary>
    public SubscriptionStatus? PreviousStatus { get; set; }

    /// <summary>الحالة الجديدة</summary>
    public SubscriptionStatus? NewStatus { get; set; }

    /// <summary>الباقة السابقة</summary>
    public Guid? PreviousPlanId { get; set; }

    /// <summary>الباقة الجديدة</summary>
    public Guid? NewPlanId { get; set; }

    #endregion

    #region Financial - المالية

    /// <summary>المبلغ المرتبط بالحدث</summary>
    public decimal? Amount { get; set; }

    /// <summary>العملة</summary>
    public string? Currency { get; set; }

    /// <summary>معرف الفاتورة المرتبطة</summary>
    public Guid? InvoiceId { get; set; }

    /// <summary>معرف عملية الدفع</summary>
    public string? PaymentId { get; set; }

    #endregion

    #region Source - المصدر

    /// <summary>المستخدم الذي قام بالإجراء</summary>
    public string? PerformedBy { get; set; }

    /// <summary>نوع المستخدم (vendor, admin, system)</summary>
    public string? PerformedByType { get; set; }

    /// <summary>عنوان IP</summary>
    public string? IpAddress { get; set; }

    /// <summary>User Agent</summary>
    public string? UserAgent { get; set; }

    #endregion

    #region Metadata - بيانات إضافية

    /// <summary>بيانات إضافية JSON</summary>
    public string? MetadataJson { get; set; }

    /// <summary>رسالة خطأ إن وجدت</summary>
    public string? ErrorMessage { get; set; }

    #endregion

    #region Factory Methods - دوال إنشاء

    public static SubscriptionEvent Created(Guid subscriptionId, Guid vendorId, Guid planId, string? performedBy = null)
    {
        return new SubscriptionEvent
        {
            SubscriptionId = subscriptionId,
            VendorId = vendorId,
            EventType = SubscriptionEventType.Created,
            NewPlanId = planId,
            NewStatus = SubscriptionStatus.Trial,
            EventDate = DateTime.UtcNow,
            Description = "تم إنشاء الاشتراك",
            PerformedBy = performedBy,
            PerformedByType = performedBy != null ? "vendor" : "system"
        };
    }

    public static SubscriptionEvent Activated(Guid subscriptionId, Guid vendorId, decimal amount, string? paymentId = null)
    {
        return new SubscriptionEvent
        {
            SubscriptionId = subscriptionId,
            VendorId = vendorId,
            EventType = SubscriptionEventType.Activated,
            PreviousStatus = SubscriptionStatus.Trial,
            NewStatus = SubscriptionStatus.Active,
            EventDate = DateTime.UtcNow,
            Amount = amount,
            PaymentId = paymentId,
            Description = "تم تفعيل الاشتراك",
            PerformedByType = "system"
        };
    }

    public static SubscriptionEvent Renewed(Guid subscriptionId, Guid vendorId, decimal amount, string? paymentId = null)
    {
        return new SubscriptionEvent
        {
            SubscriptionId = subscriptionId,
            VendorId = vendorId,
            EventType = SubscriptionEventType.Renewed,
            EventDate = DateTime.UtcNow,
            Amount = amount,
            PaymentId = paymentId,
            Description = "تم تجديد الاشتراك",
            PerformedByType = "system"
        };
    }

    public static SubscriptionEvent Upgraded(Guid subscriptionId, Guid vendorId, Guid fromPlanId, Guid toPlanId)
    {
        return new SubscriptionEvent
        {
            SubscriptionId = subscriptionId,
            VendorId = vendorId,
            EventType = SubscriptionEventType.Upgraded,
            PreviousPlanId = fromPlanId,
            NewPlanId = toPlanId,
            EventDate = DateTime.UtcNow,
            Description = "تم ترقية الباقة",
            PerformedByType = "vendor"
        };
    }

    public static SubscriptionEvent Downgraded(Guid subscriptionId, Guid vendorId, Guid fromPlanId, Guid toPlanId)
    {
        return new SubscriptionEvent
        {
            SubscriptionId = subscriptionId,
            VendorId = vendorId,
            EventType = SubscriptionEventType.Downgraded,
            PreviousPlanId = fromPlanId,
            NewPlanId = toPlanId,
            EventDate = DateTime.UtcNow,
            Description = "تم تخفيض الباقة",
            PerformedByType = "vendor"
        };
    }

    public static SubscriptionEvent Cancelled(Guid subscriptionId, Guid vendorId, string? reason = null, string? performedBy = null)
    {
        return new SubscriptionEvent
        {
            SubscriptionId = subscriptionId,
            VendorId = vendorId,
            EventType = SubscriptionEventType.Cancelled,
            PreviousStatus = SubscriptionStatus.Active,
            NewStatus = SubscriptionStatus.Cancelled,
            EventDate = DateTime.UtcNow,
            Description = reason ?? "تم إلغاء الاشتراك",
            PerformedBy = performedBy,
            PerformedByType = performedBy != null ? "vendor" : "system"
        };
    }

    public static SubscriptionEvent PaymentFailed(Guid subscriptionId, Guid vendorId, string? errorMessage = null)
    {
        return new SubscriptionEvent
        {
            SubscriptionId = subscriptionId,
            VendorId = vendorId,
            EventType = SubscriptionEventType.PaymentFailed,
            EventDate = DateTime.UtcNow,
            Description = "فشل الدفع",
            ErrorMessage = errorMessage,
            PerformedByType = "system"
        };
    }

    #endregion
}
