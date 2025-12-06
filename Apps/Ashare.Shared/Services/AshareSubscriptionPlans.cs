using ACommerce.Subscriptions.Entities;
using ACommerce.Subscriptions.Enums;

namespace Ashare.Shared.Services;

/// <summary>
/// باقات عشير للاشتراكات
/// تسعيرة خاصة بتطبيق عشير لمشاركة المساحات
/// </summary>
public static class AshareSubscriptionPlans
{
    #region باقات المنشآت والوسطاء العقاريين

    /// <summary>
    /// باقة المنشآت والوسطاء - سنوية مفتوحة
    /// 4800 ريال سنوي + 3% عمولة
    /// </summary>
    public static SubscriptionPlan BusinessAnnual => new()
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
        Name = "المنشآت - سنوي",
        NameEn = "Business - Annual",
        Slug = "business-annual",
        Description = "للمنشآت والوسطاء العقاريين - اشتراك سنوي مفتوح",
        DescriptionEn = "For businesses and real estate brokers - unlimited annual subscription",
        Icon = "bi-building",
        Color = "#8B5CF6",
        SortOrder = 1,
        IsActive = true,
        IsDefault = false,
        IsRecommended = true,

        // التسعير
        MonthlyPrice = 400, // 4800/12
        AnnualPrice = 4800,
        Currency = "SAR",
        TrialDays = 7,
        GracePeriodDays = 7,

        // الحدود - مفتوح
        MaxListings = -1,
        MaxImagesPerListing = 20,
        MaxFeaturedListings = -1,
        StorageLimitMB = -1,
        MaxTeamMembers = 10,
        MaxMonthlyMessages = -1,
        MaxMonthlyApiCalls = -1,
        ListingDurationDays = 0,

        // العمولة - 3% من عقد السنة الأولى
        CommissionType = CommissionType.Percentage,
        CommissionPercentage = 3,
        CommissionFixedAmount = 0,

        // المميزات
        HasVerifiedBadge = true,
        SearchPriorityBoost = 10,
        AnalyticsLevel = AnalyticsLevel.Full,
        SupportLevel = SupportLevel.Priority,
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
    /// باقة المنشآت والوسطاء - شهرية مفتوحة
    /// 480 ريال شهري + 3% عمولة
    /// </summary>
    public static SubscriptionPlan BusinessMonthly => new()
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
        Name = "المنشآت - شهري",
        NameEn = "Business - Monthly",
        Slug = "business-monthly",
        Description = "للمنشآت والوسطاء العقاريين - اشتراك شهري مفتوح",
        DescriptionEn = "For businesses and real estate brokers - unlimited monthly subscription",
        Icon = "bi-building",
        Color = "#7C3AED",
        SortOrder = 2,
        IsActive = true,
        IsDefault = false,
        IsRecommended = false,

        // التسعير
        MonthlyPrice = 480,
        AnnualPrice = 4800,
        Currency = "SAR",
        TrialDays = 0,
        GracePeriodDays = 3,

        // الحدود - مفتوح
        MaxListings = -1,
        MaxImagesPerListing = 20,
        MaxFeaturedListings = -1,
        StorageLimitMB = -1,
        MaxTeamMembers = 10,
        MaxMonthlyMessages = -1,
        MaxMonthlyApiCalls = -1,
        ListingDurationDays = 0,

        // العمولة - 3%
        CommissionType = CommissionType.Percentage,
        CommissionPercentage = 3,
        CommissionFixedAmount = 0,

        // المميزات
        HasVerifiedBadge = true,
        SearchPriorityBoost = 8,
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
    /// باقة المنشآت والوسطاء - 5 عقارات
    /// 140 ريال + 3% عمولة
    /// </summary>
    public static SubscriptionPlan BusinessPack5 => new()
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
        Name = "المنشآت - 5 عقارات",
        NameEn = "Business - 5 Properties",
        Slug = "business-pack-5",
        Description = "للمنشآت - باقة 5 عقارات",
        DescriptionEn = "For businesses - 5 properties pack",
        Icon = "bi-box-seam",
        Color = "#6366F1",
        SortOrder = 3,
        IsActive = true,
        IsDefault = false,
        IsRecommended = false,

        // التسعير - مبلغ لمرة واحدة
        MonthlyPrice = 140,
        Currency = "SAR",
        TrialDays = 0,
        GracePeriodDays = 0,

        // الحدود - 5 عقارات
        MaxListings = 5,
        MaxImagesPerListing = 15,
        MaxFeaturedListings = 1,
        StorageLimitMB = 500,
        MaxTeamMembers = 3,
        MaxMonthlyMessages = -1,
        MaxMonthlyApiCalls = 500,
        ListingDurationDays = 90,

        // العمولة - 3%
        CommissionType = CommissionType.Percentage,
        CommissionPercentage = 3,
        CommissionFixedAmount = 0,

        // المميزات
        HasVerifiedBadge = false,
        SearchPriorityBoost = 5,
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

    #endregion

    #region باقات الأفراد

    /// <summary>
    /// باقة الأفراد - سنوية مفتوحة
    /// 2000 ريال سنوي + 3% عمولة
    /// </summary>
    public static SubscriptionPlan IndividualAnnual => new()
    {
        Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
        Name = "الأفراد - سنوي",
        NameEn = "Individual - Annual",
        Slug = "individual-annual",
        Description = "للأفراد - اشتراك سنوي مفتوح",
        DescriptionEn = "For individuals - unlimited annual subscription",
        Icon = "bi-person",
        Color = "#10B981",
        SortOrder = 4,
        IsActive = true,
        IsDefault = false,
        IsRecommended = true,

        // التسعير
        MonthlyPrice = 167, // ~2000/12
        AnnualPrice = 2000,
        Currency = "SAR",
        TrialDays = 7,
        GracePeriodDays = 7,

        // الحدود - مفتوح
        MaxListings = -1,
        MaxImagesPerListing = 15,
        MaxFeaturedListings = -1,
        StorageLimitMB = -1,
        MaxTeamMembers = 1,
        MaxMonthlyMessages = -1,
        MaxMonthlyApiCalls = -1,
        ListingDurationDays = 0,

        // العمولة - 3%
        CommissionType = CommissionType.Percentage,
        CommissionPercentage = 3,
        CommissionFixedAmount = 0,

        // المميزات
        HasVerifiedBadge = true,
        SearchPriorityBoost = 7,
        AnalyticsLevel = AnalyticsLevel.Advanced,
        SupportLevel = SupportLevel.Priority,
        AllowDirectMessages = true,
        AllowApiAccess = false,
        AllowCustomStorePage = true,
        AllowPromotionalTools = true,
        AllowDataExport = true,
        RemoveBranding = false,
        EmailReports = true,
        PushNotifications = true,

        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// باقة الأفراد - شهرية مفتوحة
    /// 200 ريال شهري + 3% عمولة
    /// </summary>
    public static SubscriptionPlan IndividualMonthly => new()
    {
        Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
        Name = "الأفراد - شهري",
        NameEn = "Individual - Monthly",
        Slug = "individual-monthly",
        Description = "للأفراد - اشتراك شهري مفتوح",
        DescriptionEn = "For individuals - unlimited monthly subscription",
        Icon = "bi-person",
        Color = "#059669",
        SortOrder = 5,
        IsActive = true,
        IsDefault = false,
        IsRecommended = false,

        // التسعير
        MonthlyPrice = 200,
        AnnualPrice = 2000,
        Currency = "SAR",
        TrialDays = 0,
        GracePeriodDays = 3,

        // الحدود - مفتوح
        MaxListings = -1,
        MaxImagesPerListing = 15,
        MaxFeaturedListings = -1,
        StorageLimitMB = -1,
        MaxTeamMembers = 1,
        MaxMonthlyMessages = -1,
        MaxMonthlyApiCalls = -1,
        ListingDurationDays = 0,

        // العمولة - 3%
        CommissionType = CommissionType.Percentage,
        CommissionPercentage = 3,
        CommissionFixedAmount = 0,

        // المميزات
        HasVerifiedBadge = false,
        SearchPriorityBoost = 5,
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
    /// باقة الأفراد - 5 عقارات
    /// 75 ريال + 3% عمولة
    /// </summary>
    public static SubscriptionPlan IndividualPack5 => new()
    {
        Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
        Name = "الأفراد - 5 عقارات",
        NameEn = "Individual - 5 Properties",
        Slug = "individual-pack-5",
        Description = "للأفراد - باقة 5 عقارات",
        DescriptionEn = "For individuals - 5 properties pack",
        Icon = "bi-box",
        Color = "#047857",
        SortOrder = 6,
        IsActive = true,
        IsDefault = true, // الباقة الافتراضية
        IsRecommended = false,

        // التسعير - مبلغ لمرة واحدة
        MonthlyPrice = 75,
        Currency = "SAR",
        TrialDays = 0,
        GracePeriodDays = 0,

        // الحدود - 5 عقارات
        MaxListings = 5,
        MaxImagesPerListing = 10,
        MaxFeaturedListings = 0,
        StorageLimitMB = 200,
        MaxTeamMembers = 1,
        MaxMonthlyMessages = -1,
        MaxMonthlyApiCalls = 100,
        ListingDurationDays = 60,

        // العمولة - 3%
        CommissionType = CommissionType.Percentage,
        CommissionPercentage = 3,
        CommissionFixedAmount = 0,

        // المميزات
        HasVerifiedBadge = false,
        SearchPriorityBoost = 0,
        AnalyticsLevel = AnalyticsLevel.Basic,
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

    #endregion

    #region باقات خاصة

    /// <summary>
    /// باقة الباحث عن شريك
    /// 49 ريال - الدفع بعد إيجاد الشريك
    /// </summary>
    public static SubscriptionPlan PartnerSeeker => new()
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
        Name = "الباحث عن شريك",
        NameEn = "Partner Seeker",
        Slug = "partner-seeker",
        Description = "للمستأجرين الباحثين عن شريك سكن - الدفع بعد إيجاد الشريك",
        DescriptionEn = "For tenants seeking roommates - pay after finding a partner",
        Icon = "bi-people",
        Color = "#F59E0B",
        SortOrder = 7,
        IsActive = true,
        IsDefault = false,
        IsRecommended = false,

        // التسعير - الدفع عند النجاح
        MonthlyPrice = 49,
        Currency = "SAR",
        TrialDays = 0,
        GracePeriodDays = 0,

        // الحدود
        MaxListings = 1,
        MaxImagesPerListing = 5,
        MaxFeaturedListings = 0,
        StorageLimitMB = 50,
        MaxTeamMembers = 1,
        MaxMonthlyMessages = -1,
        MaxMonthlyApiCalls = 50,
        ListingDurationDays = 30,

        // بدون عمولة - رسم ثابت فقط
        CommissionType = CommissionType.Fixed,
        CommissionPercentage = 0,
        CommissionFixedAmount = 49,

        // المميزات
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
    /// باقة العقارات الإدارية والتجارية
    /// 5% من قيمة الإيجار
    /// </summary>
    public static SubscriptionPlan CommercialAdmin => new()
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
        Name = "الإداري والتجاري",
        NameEn = "Commercial & Administrative",
        Slug = "commercial-admin",
        Description = "للعقارات الإدارية والتجارية - 5% من قيمة الإيجار بعد التوقيع",
        DescriptionEn = "For commercial and administrative properties - 5% of rent after signing",
        Icon = "bi-shop",
        Color = "#EF4444",
        SortOrder = 8,
        IsActive = true,
        IsDefault = false,
        IsRecommended = false,

        // بدون رسوم اشتراك - عمولة فقط
        MonthlyPrice = 0,
        Currency = "SAR",
        TrialDays = 0,
        GracePeriodDays = 0,

        // الحدود - مفتوح
        MaxListings = -1,
        MaxImagesPerListing = 20,
        MaxFeaturedListings = 5,
        StorageLimitMB = 1000,
        MaxTeamMembers = 5,
        MaxMonthlyMessages = -1,
        MaxMonthlyApiCalls = 500,
        ListingDurationDays = 0,

        // العمولة - 5% من قيمة الإيجار
        CommissionType = CommissionType.Percentage,
        CommissionPercentage = 5,
        CommissionFixedAmount = 0,

        // المميزات
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
    /// خدمة العقد في المنصة (إضافية)
    /// 129 ريال - اختياري
    /// </summary>
    public static SubscriptionPlan ContractService => new()
    {
        Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
        Name = "عقد المنصة",
        NameEn = "Platform Contract",
        Slug = "platform-contract",
        Description = "خدمة توثيق العقد عبر المنصة - اختياري",
        DescriptionEn = "Platform contract documentation service - optional",
        Icon = "bi-file-earmark-text",
        Color = "#6366F1",
        SortOrder = 9,
        IsActive = true,
        IsDefault = false,
        IsRecommended = false,

        // رسم لمرة واحدة
        MonthlyPrice = 129,
        Currency = "SAR",
        TrialDays = 0,
        GracePeriodDays = 0,

        // الحدود
        MaxListings = 0,
        MaxImagesPerListing = 0,
        MaxFeaturedListings = 0,
        StorageLimitMB = 100,
        MaxTeamMembers = 1,
        MaxMonthlyMessages = 0,
        MaxMonthlyApiCalls = 0,
        ListingDurationDays = 0,

        // بدون عمولة
        CommissionType = CommissionType.Fixed,
        CommissionPercentage = 0,
        CommissionFixedAmount = 129,

        // المميزات
        HasVerifiedBadge = false,
        SearchPriorityBoost = 0,
        AnalyticsLevel = AnalyticsLevel.None,
        SupportLevel = SupportLevel.Standard,
        AllowDirectMessages = false,
        AllowApiAccess = false,
        AllowCustomStorePage = false,
        AllowPromotionalTools = false,
        AllowDataExport = true,
        RemoveBranding = false,
        EmailReports = false,
        PushNotifications = true,

        CreatedAt = DateTime.UtcNow
    };

    #endregion

    #region Helper Methods

    /// <summary>
    /// جميع الباقات المتاحة لعشير
    /// </summary>
    public static List<SubscriptionPlan> GetAll() =>
    [
        // باقات المنشآت
        BusinessAnnual,
        BusinessMonthly,
        BusinessPack5,
        // باقات الأفراد
        IndividualAnnual,
        IndividualMonthly,
        IndividualPack5,
        // باقات خاصة
        PartnerSeeker,
        CommercialAdmin,
        ContractService
    ];

    /// <summary>
    /// باقات المنشآت والوسطاء
    /// </summary>
    public static List<SubscriptionPlan> GetBusinessPlans() =>
    [
        BusinessAnnual,
        BusinessMonthly,
        BusinessPack5
    ];

    /// <summary>
    /// باقات الأفراد
    /// </summary>
    public static List<SubscriptionPlan> GetIndividualPlans() =>
    [
        IndividualAnnual,
        IndividualMonthly,
        IndividualPack5
    ];

    /// <summary>
    /// الحصول على باقة بواسطة المعرف
    /// </summary>
    public static SubscriptionPlan? GetBySlug(string slug) => slug.ToLower() switch
    {
        "business-annual" => BusinessAnnual,
        "business-monthly" => BusinessMonthly,
        "business-pack-5" => BusinessPack5,
        "individual-annual" => IndividualAnnual,
        "individual-monthly" => IndividualMonthly,
        "individual-pack-5" => IndividualPack5,
        "partner-seeker" => PartnerSeeker,
        "commercial-admin" => CommercialAdmin,
        "platform-contract" => ContractService,
        _ => null
    };

    /// <summary>
    /// الحصول على باقة بواسطة المعرف الفريد
    /// </summary>
    public static SubscriptionPlan? GetById(Guid id)
    {
        return GetAll().FirstOrDefault(p => p.Id == id);
    }

    #endregion
}
