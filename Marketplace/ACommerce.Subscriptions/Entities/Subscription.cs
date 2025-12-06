using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.Subscriptions.Enums;

namespace ACommerce.Subscriptions.Entities;

/// <summary>
/// اشتراك المزود - ربط المزود بباقة معينة
/// </summary>
public class Subscription : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    #region Relations - العلاقات

    /// <summary>معرف المزود/البائع</summary>
    public Guid VendorId { get; set; }

    /// <summary>معرف خطة الاشتراك</summary>
    public Guid PlanId { get; set; }

    /// <summary>خطة الاشتراك</summary>
    public SubscriptionPlan? Plan { get; set; }

    #endregion

    #region Status & Dates - الحالة والتواريخ

    /// <summary>حالة الاشتراك</summary>
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;

    /// <summary>دورة الفوترة الحالية</summary>
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

    /// <summary>تاريخ بدء الاشتراك</summary>
    public DateTime StartDate { get; set; }

    /// <summary>تاريخ انتهاء الفترة الحالية</summary>
    public DateTime CurrentPeriodEnd { get; set; }

    /// <summary>تاريخ انتهاء الفترة التجريبية</summary>
    public DateTime? TrialEndDate { get; set; }

    /// <summary>تاريخ الإلغاء (إن تم الإلغاء)</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>سبب الإلغاء</summary>
    public string? CancellationReason { get; set; }

    /// <summary>تاريخ آخر دفعة ناجحة</summary>
    public DateTime? LastPaymentDate { get; set; }

    /// <summary>تاريخ الدفعة القادمة</summary>
    public DateTime? NextPaymentDate { get; set; }

    #endregion

    #region Pricing Snapshot - لقطة التسعير

    /// <summary>السعر المتفق عليه (لقطة من وقت الاشتراك)</summary>
    public decimal Price { get; set; }

    /// <summary>العملة</summary>
    public string Currency { get; set; } = "SAR";

    /// <summary>نسبة العمولة المتفق عليها</summary>
    public decimal CommissionPercentage { get; set; }

    /// <summary>مبلغ العمولة الثابت</summary>
    public decimal CommissionFixedAmount { get; set; }

    /// <summary>نوع العمولة</summary>
    public CommissionType CommissionType { get; set; }

    #endregion

    #region Limits Snapshot - لقطة الحدود

    /// <summary>الحد الأقصى للعروض</summary>
    public int MaxListings { get; set; }

    /// <summary>الحد الأقصى للصور لكل عرض</summary>
    public int MaxImagesPerListing { get; set; }

    /// <summary>الحد الأقصى للعروض المميزة</summary>
    public int MaxFeaturedListings { get; set; }

    /// <summary>مساحة التخزين بالميجابايت</summary>
    public int StorageLimitMB { get; set; }

    #endregion

    #region Usage Tracking - تتبع الاستخدام

    /// <summary>عدد العروض الحالية</summary>
    public int CurrentListingsCount { get; set; }

    /// <summary>عدد العروض المميزة الحالية</summary>
    public int CurrentFeaturedListingsCount { get; set; }

    /// <summary>المساحة المستخدمة بالميجابايت</summary>
    public decimal CurrentStorageUsedMB { get; set; }

    /// <summary>عدد الرسائل هذا الشهر</summary>
    public int CurrentMonthMessages { get; set; }

    /// <summary>عدد طلبات API هذا الشهر</summary>
    public int CurrentMonthApiCalls { get; set; }

    /// <summary>تاريخ آخر تصفير للعدادات الشهرية</summary>
    public DateTime? LastUsageResetDate { get; set; }

    #endregion

    #region Settings - الإعدادات

    /// <summary>التجديد التلقائي</summary>
    public bool AutoRenew { get; set; } = true;

    /// <summary>معرف طريقة الدفع المحفوظة</summary>
    public string? PaymentMethodId { get; set; }

    /// <summary>البريد لإرسال الفواتير</summary>
    public string? BillingEmail { get; set; }

    #endregion

    #region Promotion - العروض الترويجية

    /// <summary>كود الخصم المستخدم</summary>
    public string? CouponCode { get; set; }

    /// <summary>نسبة الخصم</summary>
    public decimal? DiscountPercentage { get; set; }

    /// <summary>تاريخ انتهاء الخصم</summary>
    public DateTime? DiscountEndDate { get; set; }

    #endregion

    #region Metadata - بيانات إضافية

    /// <summary>بيانات إضافية</summary>
    [NotMapped]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>JSON للبيانات الإضافية (للتخزين)</summary>
    public string? MetadataJson { get; set; }

    /// <summary>ملاحظات داخلية</summary>
    public string? InternalNotes { get; set; }

    #endregion

    #region Navigation - العلاقات

    /// <summary>الفواتير</summary>
    public List<SubscriptionInvoice>? Invoices { get; set; }

    /// <summary>سجل الأحداث</summary>
    public List<SubscriptionEvent>? Events { get; set; }

    #endregion

    #region Helper Properties - خصائص مساعدة

    /// <summary>هل الاشتراك في الفترة التجريبية</summary>
    [NotMapped]
    public bool IsInTrial => Status == SubscriptionStatus.Trial &&
                             TrialEndDate.HasValue &&
                             DateTime.UtcNow < TrialEndDate.Value;

    /// <summary>هل الاشتراك نشط</summary>
    [NotMapped]
    public bool IsActive => Status == SubscriptionStatus.Active || IsInTrial;

    /// <summary>الأيام المتبقية في الفترة الحالية</summary>
    [NotMapped]
    public int DaysRemaining => Math.Max(0, (CurrentPeriodEnd - DateTime.UtcNow).Days);

    /// <summary>نسبة استخدام العروض</summary>
    [NotMapped]
    public decimal ListingsUsagePercentage => MaxListings <= 0 ? 0 :
        Math.Min(100, (decimal)CurrentListingsCount / MaxListings * 100);

    /// <summary>هل وصل للحد الأقصى من العروض</summary>
    [NotMapped]
    public bool HasReachedListingsLimit => MaxListings != -1 && CurrentListingsCount >= MaxListings;

    /// <summary>هل وصل للحد الأقصى من التخزين</summary>
    [NotMapped]
    public bool HasReachedStorageLimit => StorageLimitMB != -1 && CurrentStorageUsedMB >= StorageLimitMB;

    #endregion

    #region Helper Methods - دوال مساعدة

    /// <summary>يمكن إضافة عرض جديد</summary>
    public bool CanAddListing() => IsActive && !HasReachedListingsLimit;

    /// <summary>يمكن إضافة عرض مميز</summary>
    public bool CanAddFeaturedListing() => IsActive &&
        (MaxFeaturedListings == -1 || CurrentFeaturedListingsCount < MaxFeaturedListings);

    /// <summary>يمكن رفع ملف بحجم معين</summary>
    public bool CanUploadFile(decimal fileSizeMB) => IsActive &&
        (StorageLimitMB == -1 || CurrentStorageUsedMB + fileSizeMB <= StorageLimitMB);

    /// <summary>حساب العمولة لمبلغ معين</summary>
    public decimal CalculateCommission(decimal amount)
    {
        decimal commission = CommissionType switch
        {
            CommissionType.Percentage => amount * (CommissionPercentage / 100),
            CommissionType.Fixed => CommissionFixedAmount,
            CommissionType.Hybrid => (amount * (CommissionPercentage / 100)) + CommissionFixedAmount,
            _ => 0
        };

        return Math.Round(commission, 2);
    }

    #endregion
}
