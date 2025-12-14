using ACommerce.Subscriptions.Entities;
using ACommerce.Subscriptions.Enums;

namespace ACommerce.Subscriptions;

/// <summary>
/// الباقات الافتراضية للنظام
/// </summary>
public static class DefaultPlans
{
    /// <summary>
    /// الباقة المجانية
    /// </summary>
    public static SubscriptionPlan Free => new()
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Name = "مجاني",
        NameEn = "Free",
        Slug = "free",
        Description = "ابدأ مجاناً واستكشف المنصة",
        DescriptionEn = "Start free and explore the platform",
        Icon = "bi-gift",
        Color = "#6B7280",
        SortOrder = 1,
        IsActive = true,
        IsDefault = true,
        IsRecommended = false,

        // Pricing
        MonthlyPrice = 0,
        Currency = "SAR",
        TrialDays = 0,
        GracePeriodDays = 0,

        // Limits
        MaxListings = 3,
        MaxImagesPerListing = 3,
        MaxFeaturedListings = 0,
        StorageLimitMB = 50,
        MaxTeamMembers = 1,
        MaxMonthlyMessages = 10,
        MaxMonthlyApiCalls = 0,
        ListingDurationDays = 30,

        // Commission
        CommissionType = CommissionType.Percentage,
        CommissionPercentage = 15,
        CommissionFixedAmount = 0,

        // Features
        HasVerifiedBadge = false,
        SearchPriorityBoost = 0,
        AnalyticsLevel = AnalyticsLevel.None,
        SupportLevel = SupportLevel.Basic,
        AllowDirectMessages = true,
        AllowApiAccess = false,
        AllowCustomStorePage = false,
        AllowPromotionalTools = false,
        AllowDataExport = false,
        RemoveBranding = false,
        EmailReports = false,
        PushNotifications = true,

        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// الباقة الأساسية
    /// </summary>
    public static SubscriptionPlan Basic => new()
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
        Name = "أساسي",
        NameEn = "Basic",
        Slug = "basic",
        Description = "للمزودين الجدد الذين يريدون النمو",
        DescriptionEn = "For new providers who want to grow",
        Icon = "bi-star",
        Color = "#3B82F6",
        SortOrder = 2,
        IsActive = true,
        IsDefault = false,
        IsRecommended = false,

        // Pricing
        MonthlyPrice = 99,
        QuarterlyPrice = 267, // ~10% off
        SemiAnnualPrice = 504, // ~15% off
        AnnualPrice = 950, // ~20% off
        Currency = "SAR",
        TrialDays = 14,
        GracePeriodDays = 3,

        // Limits
        MaxListings = 25,
        MaxImagesPerListing = 10,
        MaxFeaturedListings = 2,
        StorageLimitMB = 500,
        MaxTeamMembers = 2,
        MaxMonthlyMessages = 100,
        MaxMonthlyApiCalls = 100,
        ListingDurationDays = 60,

        // Commission
        CommissionType = CommissionType.Percentage,
        CommissionPercentage = 10,
        CommissionFixedAmount = 0,

        // Features
        HasVerifiedBadge = false,
        SearchPriorityBoost = 1,
        AnalyticsLevel = AnalyticsLevel.Basic,
        SupportLevel = SupportLevel.Standard,
        AllowDirectMessages = true,
        AllowApiAccess = false,
        AllowCustomStorePage = false,
        AllowPromotionalTools = true,
        AllowDataExport = false,
        RemoveBranding = false,
        EmailReports = false,
        PushNotifications = true,

        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// الباقة الاحترافية
    /// </summary>
    public static SubscriptionPlan Pro => new()
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
        Name = "احترافي",
        NameEn = "Pro",
        Slug = "pro",
        Description = "للمزودين المحترفين الذين يريدون التميز",
        DescriptionEn = "For professional providers who want to excel",
        Icon = "bi-award",
        Color = "#8B5CF6",
        SortOrder = 3,
        IsActive = true,
        IsDefault = false,
        IsRecommended = true,

        // Pricing
        MonthlyPrice = 299,
        QuarterlyPrice = 807, // ~10% off
        SemiAnnualPrice = 1524, // ~15% off
        AnnualPrice = 2870, // ~20% off
        Currency = "SAR",
        TrialDays = 14,
        GracePeriodDays = 7,

        // Limits
        MaxListings = 100,
        MaxImagesPerListing = 20,
        MaxFeaturedListings = 10,
        StorageLimitMB = 2000,
        MaxTeamMembers = 5,
        MaxMonthlyMessages = -1, // Unlimited
        MaxMonthlyApiCalls = 1000,
        ListingDurationDays = 90,

        // Commission
        CommissionType = CommissionType.Percentage,
        CommissionPercentage = 7,
        CommissionFixedAmount = 0,

        // Features
        HasVerifiedBadge = true,
        SearchPriorityBoost = 5,
        AnalyticsLevel = AnalyticsLevel.Advanced,
        SupportLevel = SupportLevel.Priority,
        AllowDirectMessages = true,
        AllowApiAccess = true,
        AllowCustomStorePage = true,
        AllowPromotionalTools = true,
        AllowDataExport = true,
        RemoveBranding = false,
        EmailReports = true,
        PushNotifications = true,

        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// باقة الأعمال
    /// </summary>
    public static SubscriptionPlan Business => new()
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
        Name = "أعمال",
        NameEn = "Business",
        Slug = "business",
        Description = "للشركات والمؤسسات الكبيرة",
        DescriptionEn = "For large companies and enterprises",
        Icon = "bi-building",
        Color = "#F59E0B",
        SortOrder = 4,
        IsActive = true,
        IsDefault = false,
        IsRecommended = false,

        // Pricing
        MonthlyPrice = 799,
        QuarterlyPrice = 2157, // ~10% off
        SemiAnnualPrice = 4075, // ~15% off
        AnnualPrice = 7670, // ~20% off
        Currency = "SAR",
        TrialDays = 30,
        GracePeriodDays = 14,

        // Limits - Unlimited
        MaxListings = -1,
        MaxImagesPerListing = 50,
        MaxFeaturedListings = -1,
        StorageLimitMB = -1,
        MaxTeamMembers = -1,
        MaxMonthlyMessages = -1,
        MaxMonthlyApiCalls = -1,
        ListingDurationDays = 0, // Never expires

        // Commission
        CommissionType = CommissionType.Percentage,
        CommissionPercentage = 5,
        CommissionFixedAmount = 0,
        MinCommission = 1,
        MaxCommission = 500,

        // Features - All enabled
        HasVerifiedBadge = true,
        SearchPriorityBoost = 10,
        AnalyticsLevel = AnalyticsLevel.Full,
        SupportLevel = SupportLevel.Dedicated,
        AllowDirectMessages = true,
        AllowApiAccess = true,
        AllowCustomStorePage = true,
        AllowPromotionalTools = true,
        AllowDataExport = true,
        RemoveBranding = true,
        EmailReports = true,
        PushNotifications = true,

        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// الحصول على جميع الباقات الافتراضية
    /// </summary>
    public static List<SubscriptionPlan> GetAll() =>
    [
        Free,
        Basic,
        Pro,
        Business
    ];

    /// <summary>
    /// الحصول على باقة بواسطة المعرف الفريد
    /// </summary>
    public static SubscriptionPlan? GetBySlug(string slug) => slug.ToLower() switch
    {
        "free" => Free,
        "basic" => Basic,
        "pro" => Pro,
        "business" => Business,
        _ => null
    };
}
