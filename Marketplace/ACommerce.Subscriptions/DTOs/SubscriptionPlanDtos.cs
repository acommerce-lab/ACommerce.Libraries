using ACommerce.Subscriptions.Enums;

namespace ACommerce.Subscriptions.DTOs;

/// <summary>
/// DTO لعرض خطة الاشتراك
/// </summary>
public record SubscriptionPlanDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? NameEn { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public string? DescriptionEn { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public int SortOrder { get; init; }
    public bool IsDefault { get; init; }
    public bool IsRecommended { get; init; }

    // Pricing
    public decimal MonthlyPrice { get; init; }
    public decimal? QuarterlyPrice { get; init; }
    public decimal? SemiAnnualPrice { get; init; }
    public decimal? AnnualPrice { get; init; }
    public string Currency { get; init; } = "SAR";
    public int TrialDays { get; init; }

    // Limits
    public int MaxListings { get; init; }
    public int MaxImagesPerListing { get; init; }
    public int MaxFeaturedListings { get; init; }
    public int StorageLimitMB { get; init; }
    public int MaxTeamMembers { get; init; }
    public int ListingDurationDays { get; init; }

    // Commission
    public CommissionType CommissionType { get; init; }
    public decimal CommissionPercentage { get; init; }
    public decimal CommissionFixedAmount { get; init; }

    // Features
    public bool HasVerifiedBadge { get; init; }
    public int SearchPriorityBoost { get; init; }
    public AnalyticsLevel AnalyticsLevel { get; init; }
    public SupportLevel SupportLevel { get; init; }
    public bool AllowDirectMessages { get; init; }
    public bool AllowApiAccess { get; init; }
    public bool AllowCustomStorePage { get; init; }
    public bool AllowPromotionalTools { get; init; }
    public bool AllowDataExport { get; init; }
    public bool RemoveBranding { get; init; }

    // Computed
    public bool IsUnlimitedListings => MaxListings == -1;
    public decimal AnnualSavings => MonthlyPrice * 12 - (AnnualPrice ?? MonthlyPrice * 12);
    public decimal AnnualSavingsPercentage => AnnualPrice.HasValue
        ? Math.Round((1 - AnnualPrice.Value / (MonthlyPrice * 12)) * 100, 0)
        : 0;
}

/// <summary>
/// DTO لعرض خطة الاشتراك بشكل مختصر (للقوائم)
/// </summary>
public record SubscriptionPlanSummaryDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? NameEn { get; init; }
    public required string Slug { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public decimal MonthlyPrice { get; init; }
    public string Currency { get; init; } = "SAR";
    public int MaxListings { get; init; }
    public decimal CommissionPercentage { get; init; }
    public bool IsRecommended { get; init; }
    public bool IsDefault { get; init; }
}

/// <summary>
/// DTO لإنشاء خطة اشتراك جديدة
/// </summary>
public record CreateSubscriptionPlanDto
{
    public required string Name { get; init; }
    public string? NameEn { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public string? DescriptionEn { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public int SortOrder { get; init; }
    public bool IsDefault { get; init; }
    public bool IsRecommended { get; init; }

    // Pricing
    public decimal MonthlyPrice { get; init; }
    public decimal? QuarterlyPrice { get; init; }
    public decimal? SemiAnnualPrice { get; init; }
    public decimal? AnnualPrice { get; init; }
    public string Currency { get; init; } = "SAR";
    public int TrialDays { get; init; }
    public int GracePeriodDays { get; init; } = 3;

    // Limits
    public int MaxListings { get; init; }
    public int MaxImagesPerListing { get; init; } = 5;
    public int MaxFeaturedListings { get; init; }
    public int StorageLimitMB { get; init; } = 500;
    public int MaxTeamMembers { get; init; } = 1;
    public int MaxMonthlyMessages { get; init; } = -1;
    public int MaxMonthlyApiCalls { get; init; }
    public int ListingDurationDays { get; init; }

    // Commission
    public CommissionType CommissionType { get; init; } = CommissionType.Percentage;
    public decimal CommissionPercentage { get; init; }
    public decimal CommissionFixedAmount { get; init; }
    public decimal? MinCommission { get; init; }
    public decimal? MaxCommission { get; init; }

    // Features
    public bool HasVerifiedBadge { get; init; }
    public int SearchPriorityBoost { get; init; }
    public AnalyticsLevel AnalyticsLevel { get; init; } = AnalyticsLevel.Basic;
    public SupportLevel SupportLevel { get; init; } = SupportLevel.Basic;
    public bool AllowDirectMessages { get; init; } = true;
    public bool AllowApiAccess { get; init; }
    public bool AllowCustomStorePage { get; init; }
    public bool AllowPromotionalTools { get; init; }
    public bool AllowDataExport { get; init; }
    public bool RemoveBranding { get; init; }
    public bool EmailReports { get; init; }
    public bool PushNotifications { get; init; } = true;
}

/// <summary>
/// DTO لتحديث خطة اشتراك
/// </summary>
public record UpdateSubscriptionPlanDto : CreateSubscriptionPlanDto
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// DTO لمقارنة الباقات
/// </summary>
public record PlanComparisonDto
{
    public List<SubscriptionPlanDto> Plans { get; init; } = new();
    public List<PlanFeatureComparisonDto> Features { get; init; } = new();
}

/// <summary>
/// DTO لمقارنة ميزة واحدة عبر الباقات
/// </summary>
public record PlanFeatureComparisonDto
{
    public required string FeatureName { get; init; }
    public required string FeatureNameEn { get; init; }
    public string? Category { get; init; }
    public Dictionary<Guid, string> ValuesByPlan { get; init; } = new();
}
