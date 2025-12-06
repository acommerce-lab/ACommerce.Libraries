using ACommerce.Subscriptions.Enums;

namespace ACommerce.Subscriptions.DTOs;

/// <summary>
/// DTO لعرض الاشتراك
/// </summary>
public record SubscriptionDto
{
    public Guid Id { get; init; }
    public Guid VendorId { get; init; }
    public Guid PlanId { get; init; }
    public SubscriptionPlanSummaryDto? Plan { get; init; }

    // Status
    public SubscriptionStatus Status { get; init; }
    public BillingCycle BillingCycle { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime CurrentPeriodEnd { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public DateTime? CancelledAt { get; init; }
    public DateTime? NextPaymentDate { get; init; }

    // Pricing
    public decimal Price { get; init; }
    public string Currency { get; init; } = "SAR";
    public decimal CommissionPercentage { get; init; }
    public decimal CommissionFixedAmount { get; init; }
    public CommissionType CommissionType { get; init; }

    // Limits
    public int MaxListings { get; init; }
    public int MaxImagesPerListing { get; init; }
    public int MaxFeaturedListings { get; init; }
    public int StorageLimitMB { get; init; }

    // Usage
    public int CurrentListingsCount { get; init; }
    public int CurrentFeaturedListingsCount { get; init; }
    public decimal CurrentStorageUsedMB { get; init; }

    // Settings
    public bool AutoRenew { get; init; }

    // Computed
    public bool IsActive { get; init; }
    public bool IsInTrial { get; init; }
    public int DaysRemaining { get; init; }
    public decimal ListingsUsagePercentage { get; init; }
    public bool HasReachedListingsLimit { get; init; }
    public decimal StorageUsagePercentage => StorageLimitMB <= 0 ? 0 :
        Math.Min(100, CurrentStorageUsedMB / StorageLimitMB * 100);

    // Discount
    public string? CouponCode { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public DateTime? DiscountEndDate { get; init; }
}

/// <summary>
/// DTO لعرض ملخص الاشتراك (للوحة التحكم)
/// </summary>
public record SubscriptionSummaryDto
{
    public Guid Id { get; init; }
    public string? PlanName { get; init; }
    public string? PlanSlug { get; init; }
    public SubscriptionStatus Status { get; init; }
    public DateTime CurrentPeriodEnd { get; init; }
    public int DaysRemaining { get; init; }
    public int ListingsUsed { get; init; }
    public int ListingsLimit { get; init; }
    public bool IsUnlimitedListings { get; init; }
    public decimal CommissionPercentage { get; init; }
    public bool CanUpgrade { get; init; }
    public bool IsExpiringSoon { get; init; }
}

/// <summary>
/// DTO لإنشاء اشتراك جديد
/// </summary>
public record CreateSubscriptionDto
{
    public Guid VendorId { get; init; }
    public Guid PlanId { get; init; }
    public BillingCycle BillingCycle { get; init; } = BillingCycle.Monthly;
    public string? CouponCode { get; init; }
    public string? PaymentMethodId { get; init; }
    public string? BillingEmail { get; init; }
    public bool AutoRenew { get; init; } = true;
}

/// <summary>
/// DTO لترقية/تخفيض الباقة
/// </summary>
public record ChangePlanDto
{
    public Guid NewPlanId { get; init; }
    public BillingCycle? NewBillingCycle { get; init; }
    public bool ApplyImmediately { get; init; } = true;
}

/// <summary>
/// DTO لإلغاء الاشتراك
/// </summary>
public record CancelSubscriptionDto
{
    public string? Reason { get; init; }
    public bool CancelImmediately { get; init; }
    public string? Feedback { get; init; }
}

/// <summary>
/// DTO لتجديد الاشتراك
/// </summary>
public record RenewSubscriptionDto
{
    public BillingCycle? BillingCycle { get; init; }
    public string? CouponCode { get; init; }
    public string? PaymentMethodId { get; init; }
}

/// <summary>
/// DTO لتطبيق كوبون خصم
/// </summary>
public record ApplyCouponDto
{
    public required string CouponCode { get; init; }
}

/// <summary>
/// نتيجة تطبيق الكوبون
/// </summary>
public record CouponValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public decimal? DiscountAmount { get; init; }
    public decimal? NewPrice { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// DTO للتحقق من إمكانية إضافة عرض
/// </summary>
public record CanAddListingResult
{
    /// <summary>هل يمكن إضافة عرض جديد</summary>
    public bool CanAdd { get; init; }

    /// <summary>سبب عدم السماح (إذا لم يُسمح)</summary>
    public string? Reason { get; init; }

    /// <summary>عدد العروض الحالية</summary>
    public int CurrentCount { get; init; }

    /// <summary>الحد الأقصى المسموح (-1 = غير محدود)</summary>
    public int MaxAllowed { get; init; }

    /// <summary>هل يجب ترقية الباقة</summary>
    public bool ShouldUpgrade { get; init; }

    /// <summary>هل يجب الاشتراك أولاً (لا يوجد اشتراك حالي)</summary>
    public bool SubscriptionRequired { get; init; }

    /// <summary>الباقات المقترحة للترقية</summary>
    public List<SubscriptionPlanSummaryDto>? SuggestedPlans { get; init; }
}

/// <summary>
/// DTO لحساب العمولة
/// </summary>
public record CommissionCalculationDto
{
    public decimal TransactionAmount { get; init; }
    public decimal CommissionAmount { get; init; }
    public decimal VendorAmount { get; init; }
    public CommissionType CommissionType { get; init; }
    public decimal CommissionPercentage { get; init; }
    public decimal CommissionFixedAmount { get; init; }
}

/// <summary>
/// DTO لإحصائيات الاشتراك
/// </summary>
public record SubscriptionStatsDto
{
    public int TotalSubscribers { get; init; }
    public int ActiveSubscribers { get; init; }
    public int TrialSubscribers { get; init; }
    public int CancelledThisMonth { get; init; }
    public decimal MonthlyRecurringRevenue { get; init; }
    public decimal AverageRevenuePerUser { get; init; }
    public Dictionary<string, int> SubscribersByPlan { get; init; } = new();
    public Dictionary<string, decimal> RevenueByPlan { get; init; } = new();
    public decimal ChurnRate { get; init; }
    public decimal TrialConversionRate { get; init; }
}

/// <summary>
/// DTO لإحصائيات استخدام المزود
/// </summary>
public record VendorUsageStatsDto
{
    public Guid VendorId { get; init; }
    public Guid SubscriptionId { get; init; }
    public string? PlanName { get; init; }

    // Listings
    public int ListingsUsed { get; init; }
    public int ListingsLimit { get; init; }
    public decimal ListingsUsagePercentage { get; init; }

    // Featured
    public int FeaturedUsed { get; init; }
    public int FeaturedLimit { get; init; }

    // Storage
    public decimal StorageUsedMB { get; init; }
    public int StorageLimitMB { get; init; }
    public decimal StorageUsagePercentage { get; init; }

    // Messages
    public int MessagesThisMonth { get; init; }
    public int MessagesLimit { get; init; }

    // API
    public int ApiCallsThisMonth { get; init; }
    public int ApiCallsLimit { get; init; }

    // Period
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public DateTime? LastResetDate { get; init; }
}
