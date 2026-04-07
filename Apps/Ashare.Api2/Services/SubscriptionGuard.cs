using ACommerce.SharedKernel.Abstractions.Repositories;
using Ashare.Api2.Entities;

namespace Ashare.Api2.Services;

/// <summary>
/// نتيجة فحص الاشتراك قبل عملية.
/// </summary>
public record SubscriptionCheckResult(
    bool Allowed,
    string? Reason = null,
    Subscription? ActiveSubscription = null,
    Plan? Plan = null,
    int? RemainingListings = null);

/// <summary>
/// خدمة فحص الاشتراك والباقة قبل تنفيذ العمليات الحساسة.
/// تستدعى من المتحكمات قبل إنشاء عرض/تمييزه/إرسال رسالة.
/// </summary>
public class SubscriptionGuard
{
    private readonly IBaseAsyncRepository<Subscription> _subs;
    private readonly IBaseAsyncRepository<Plan> _plans;
    private readonly IBaseAsyncRepository<Listing> _listings;
    private readonly IBaseAsyncRepository<Category> _categories;

    public SubscriptionGuard(IRepositoryFactory factory)
    {
        _subs = factory.CreateRepository<Subscription>();
        _plans = factory.CreateRepository<Plan>();
        _listings = factory.CreateRepository<Listing>();
        _categories = factory.CreateRepository<Category>();
    }

    /// <summary>
    /// الحصول على الاشتراك النشط للمستخدم (إن وجد).
    /// </summary>
    public async Task<Subscription?> GetActiveAsync(Guid userId, CancellationToken ct = default)
    {
        var subs = await _subs.GetAllWithPredicateAsync(s =>
            s.UserId == userId &&
            (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial));

        return subs.FirstOrDefault(s => s.IsCurrentlyActive);
    }

    /// <summary>
    /// فحص قبل إنشاء عرض جديد:
    /// 1) يجب أن يكون لدى المستخدم اشتراك نشط
    /// 2) الفئة المطلوبة يجب أن تكون مسموحاً بها في الباقة
    /// 3) لم يتجاوز MaxListings
    /// </summary>
    public async Task<SubscriptionCheckResult> CheckCanCreateListingAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken ct = default)
    {
        var sub = await GetActiveAsync(userId, ct);
        if (sub == null)
            return new SubscriptionCheckResult(false, "no_active_subscription");

        var plan = await _plans.GetByIdAsync(sub.PlanId, ct);
        if (plan == null)
            return new SubscriptionCheckResult(false, "plan_not_found", sub);

        // فحص الفئة المسموح بها
        var category = await _categories.GetByIdAsync(categoryId, ct);
        if (category == null)
            return new SubscriptionCheckResult(false, "category_not_found", sub, plan);

        if (!string.IsNullOrWhiteSpace(plan.AllowedCategorySlugs))
        {
            var allowed = plan.AllowedCategorySlugs
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (!allowed.Contains(category.Slug, StringComparer.OrdinalIgnoreCase))
            {
                return new SubscriptionCheckResult(
                    false,
                    $"category_not_allowed_in_plan: '{category.Slug}' not in [{string.Join(",", allowed)}]",
                    sub, plan);
            }
        }

        // فحص الحد الأقصى للعروض
        if (plan.MaxListings != -1)
        {
            var currentCount = await _listings.CountAsync(
                l => l.OwnerId == userId && l.Status != ListingStatus.Closed,
                cancellationToken: ct);

            if (currentCount >= plan.MaxListings)
            {
                return new SubscriptionCheckResult(
                    false,
                    $"listings_quota_exceeded: {currentCount}/{plan.MaxListings}",
                    sub, plan, RemainingListings: 0);
            }

            return new SubscriptionCheckResult(true, null, sub, plan, plan.MaxListings - currentCount);
        }

        return new SubscriptionCheckResult(true, null, sub, plan, RemainingListings: -1);
    }

    /// <summary>
    /// فحص قبل تمييز عرض (Featured).
    /// </summary>
    public async Task<SubscriptionCheckResult> CheckCanFeatureListingAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var sub = await GetActiveAsync(userId, ct);
        if (sub == null)
            return new SubscriptionCheckResult(false, "no_active_subscription");

        var plan = await _plans.GetByIdAsync(sub.PlanId, ct);
        if (plan == null)
            return new SubscriptionCheckResult(false, "plan_not_found", sub);

        if (plan.MaxFeaturedListings == -1)
            return new SubscriptionCheckResult(true, null, sub, plan);

        if (plan.MaxFeaturedListings == 0)
            return new SubscriptionCheckResult(false, "feature_not_allowed_in_plan", sub, plan);

        var currentFeatured = await _listings.CountAsync(
            l => l.OwnerId == userId && l.IsFeatured && l.Status == ListingStatus.Published,
            cancellationToken: ct);

        if (currentFeatured >= plan.MaxFeaturedListings)
            return new SubscriptionCheckResult(
                false,
                $"featured_quota_exceeded: {currentFeatured}/{plan.MaxFeaturedListings}",
                sub, plan);

        return new SubscriptionCheckResult(true, null, sub, plan);
    }

    /// <summary>
    /// فحص قبل إرسال رسالة شهرية.
    /// </summary>
    public async Task<SubscriptionCheckResult> CheckCanSendMessageAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var sub = await GetActiveAsync(userId, ct);
        if (sub == null)
            return new SubscriptionCheckResult(false, "no_active_subscription");

        var plan = await _plans.GetByIdAsync(sub.PlanId, ct);
        if (plan == null)
            return new SubscriptionCheckResult(false, "plan_not_found", sub);

        if (!plan.AllowDirectMessages)
            return new SubscriptionCheckResult(false, "direct_messages_not_allowed", sub, plan);

        if (plan.MaxMonthlyMessages == -1)
            return new SubscriptionCheckResult(true, null, sub, plan);

        if (sub.UsedMonthlyMessages >= plan.MaxMonthlyMessages)
            return new SubscriptionCheckResult(false,
                $"monthly_messages_exceeded: {sub.UsedMonthlyMessages}/{plan.MaxMonthlyMessages}",
                sub, plan);

        return new SubscriptionCheckResult(true, null, sub, plan);
    }

    /// <summary>
    /// زيادة عدّاد الرسائل الشهري بعد إرسال ناجح.
    /// </summary>
    public async Task IncrementMessageCountAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var sub = await _subs.GetByIdAsync(subscriptionId, ct);
        if (sub == null) return;

        // إعادة تصفير شهرية
        if ((DateTime.UtcNow - sub.LastUsageReset).TotalDays >= 30)
        {
            sub.UsedMonthlyMessages = 0;
            sub.UsedMonthlyApiCalls = 0;
            sub.LastUsageReset = DateTime.UtcNow;
        }

        sub.UsedMonthlyMessages++;
        await _subs.UpdateAsync(sub, ct);
    }
}
