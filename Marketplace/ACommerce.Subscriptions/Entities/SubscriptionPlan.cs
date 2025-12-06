using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.Subscriptions.Enums;

namespace ACommerce.Subscriptions.Entities;

/// <summary>
/// خطة الاشتراك - الباقة المتاحة للمزودين
/// </summary>
public class SubscriptionPlan : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    #region Basic Info - المعلومات الأساسية

    /// <summary>اسم الباقة (مثل: مجاني، أساسي، احترافي)</summary>
    public required string Name { get; set; }

    /// <summary>الاسم الإنجليزي</summary>
    public string? NameEn { get; set; }

    /// <summary>المعرف الفريد للباقة (مثل: free, basic, pro)</summary>
    public required string Slug { get; set; }

    /// <summary>وصف الباقة</summary>
    public string? Description { get; set; }

    /// <summary>الوصف الإنجليزي</summary>
    public string? DescriptionEn { get; set; }

    /// <summary>أيقونة الباقة (Bootstrap Icons)</summary>
    public string? Icon { get; set; }

    /// <summary>لون الباقة (Hex)</summary>
    public string? Color { get; set; }

    /// <summary>ترتيب العرض</summary>
    public int SortOrder { get; set; }

    /// <summary>هل الباقة نشطة ومتاحة للاشتراك</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>هل هي الباقة الافتراضية للمزودين الجدد</summary>
    public bool IsDefault { get; set; }

    /// <summary>هل هي الباقة المميزة/الموصى بها</summary>
    public bool IsRecommended { get; set; }

    #endregion

    #region Pricing - التسعير

    /// <summary>السعر الشهري</summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>السعر ربع السنوي (خصم ~10%)</summary>
    public decimal? QuarterlyPrice { get; set; }

    /// <summary>السعر نصف السنوي (خصم ~15%)</summary>
    public decimal? SemiAnnualPrice { get; set; }

    /// <summary>السعر السنوي (خصم ~20%)</summary>
    public decimal? AnnualPrice { get; set; }

    /// <summary>العملة (SAR, USD, etc.)</summary>
    public string Currency { get; set; } = "SAR";

    /// <summary>أيام الفترة التجريبية المجانية</summary>
    public int TrialDays { get; set; }

    /// <summary>أيام فترة السماح بعد انتهاء الاشتراك</summary>
    public int GracePeriodDays { get; set; } = 3;

    #endregion

    #region Limits - الحدود

    /// <summary>الحد الأقصى للعروض (-1 = غير محدود)</summary>
    public int MaxListings { get; set; }

    /// <summary>الحد الأقصى للصور لكل عرض</summary>
    public int MaxImagesPerListing { get; set; } = 5;

    /// <summary>الحد الأقصى للعروض المميزة</summary>
    public int MaxFeaturedListings { get; set; }

    /// <summary>مساحة التخزين بالميجابايت (-1 = غير محدود)</summary>
    public int StorageLimitMB { get; set; } = 500;

    /// <summary>الحد الأقصى لأعضاء الفريق</summary>
    public int MaxTeamMembers { get; set; } = 1;

    /// <summary>الحد الأقصى للرسائل الشهرية (-1 = غير محدود)</summary>
    public int MaxMonthlyMessages { get; set; } = -1;

    /// <summary>الحد الأقصى لطلبات API الشهرية (-1 = غير محدود)</summary>
    public int MaxMonthlyApiCalls { get; set; }

    /// <summary>مدة ظهور العرض بالأيام (0 = غير محدود)</summary>
    public int ListingDurationDays { get; set; }

    #endregion

    #region Commission - العمولات

    /// <summary>نوع العمولة</summary>
    public CommissionType CommissionType { get; set; } = CommissionType.Percentage;

    /// <summary>نسبة العمولة (مثل: 10 = 10%)</summary>
    public decimal CommissionPercentage { get; set; }

    /// <summary>مبلغ العمولة الثابت لكل عملية</summary>
    public decimal CommissionFixedAmount { get; set; }

    /// <summary>الحد الأدنى للعمولة</summary>
    public decimal? MinCommission { get; set; }

    /// <summary>الحد الأقصى للعمولة</summary>
    public decimal? MaxCommission { get; set; }

    #endregion

    #region Features - المميزات

    /// <summary>شارة التحقق/المميز</summary>
    public bool HasVerifiedBadge { get; set; }

    /// <summary>أولوية في نتائج البحث (1-10)</summary>
    public int SearchPriorityBoost { get; set; }

    /// <summary>مستوى الإحصائيات</summary>
    public AnalyticsLevel AnalyticsLevel { get; set; } = AnalyticsLevel.Basic;

    /// <summary>مستوى الدعم الفني</summary>
    public SupportLevel SupportLevel { get; set; } = SupportLevel.Basic;

    /// <summary>السماح بالرسائل المباشرة</summary>
    public bool AllowDirectMessages { get; set; } = true;

    /// <summary>السماح بالوصول لـ API</summary>
    public bool AllowApiAccess { get; set; }

    /// <summary>السماح بتخصيص صفحة المتجر</summary>
    public bool AllowCustomStorePage { get; set; }

    /// <summary>السماح بأدوات الترويج</summary>
    public bool AllowPromotionalTools { get; set; }

    /// <summary>السماح بتصدير البيانات</summary>
    public bool AllowDataExport { get; set; }

    /// <summary>إزالة العلامة المائية/الإعلانات</summary>
    public bool RemoveBranding { get; set; }

    /// <summary>تقارير أداء دورية بالبريد</summary>
    public bool EmailReports { get; set; }

    /// <summary>إشعارات فورية</summary>
    public bool PushNotifications { get; set; } = true;

    #endregion

    #region Metadata - بيانات إضافية

    /// <summary>مميزات إضافية كـ JSON</summary>
    [NotMapped]
    public Dictionary<string, object>? ExtraFeatures { get; set; }

    /// <summary>بيانات إضافية</summary>
    [NotMapped]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>JSON للمميزات الإضافية (للتخزين)</summary>
    public string? ExtraFeaturesJson { get; set; }

    /// <summary>JSON للبيانات الإضافية (للتخزين)</summary>
    public string? MetadataJson { get; set; }

    #endregion

    #region Navigation - العلاقات

    /// <summary>الاشتراكات في هذه الباقة</summary>
    public List<Subscription>? Subscriptions { get; set; }

    #endregion

    #region Helper Methods - دوال مساعدة

    /// <summary>الحصول على السعر حسب دورة الفوترة</summary>
    public decimal GetPrice(BillingCycle cycle) => cycle switch
    {
        BillingCycle.Monthly => MonthlyPrice,
        BillingCycle.Quarterly => QuarterlyPrice ?? (MonthlyPrice * 3 * 0.9m),
        BillingCycle.SemiAnnual => SemiAnnualPrice ?? (MonthlyPrice * 6 * 0.85m),
        BillingCycle.Annual => AnnualPrice ?? (MonthlyPrice * 12 * 0.8m),
        _ => MonthlyPrice
    };

    /// <summary>هل الحد غير محدود</summary>
    public bool IsUnlimited(LimitType limitType) => limitType switch
    {
        LimitType.Listings => MaxListings == -1,
        LimitType.ImagesPerListing => MaxImagesPerListing == -1,
        LimitType.FeaturedListings => MaxFeaturedListings == -1,
        LimitType.StorageMB => StorageLimitMB == -1,
        LimitType.TeamMembers => MaxTeamMembers == -1,
        LimitType.MonthlyMessages => MaxMonthlyMessages == -1,
        LimitType.MonthlyApiCalls => MaxMonthlyApiCalls == -1,
        _ => false
    };

    #endregion
}
