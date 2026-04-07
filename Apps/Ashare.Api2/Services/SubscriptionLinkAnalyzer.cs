using ACommerce.OperationEngine.Core;
using ACommerce.SharedKernel.Abstractions.Repositories;
using Ashare.Api2.Entities;

namespace Ashare.Api2.Services;

/// <summary>
/// محلل ربط الاشتراك (PreAnalyzer):
///
/// يطبّق نموذج محاسبي حقيقي:
///   - كل اشتراك في باقة هو "قيد رصيد" (يفتح حصة)
///   - كل عرض هو "قيد عكسي" (يستهلك من حصة)
///   - الربط FIFO: نأخذ أقدم اشتراك له رصيد + يطابق فئة العرض
///
/// عند النجاح يضع في الـ context:
///   - linked_subscription: Subscription
///   - linked_plan: Plan
/// </summary>
public class SubscriptionLinkAnalyzer : IOperationAnalyzer
{
    private readonly IRepositoryFactory _factory;
    private readonly Guid _userId;
    private readonly Guid _categoryId;

    public string Name => "SubscriptionLinkAnalyzer";

    public IReadOnlyList<string> WatchedTagKeys => new[] { "subscription_check" };

    public SubscriptionLinkAnalyzer(IRepositoryFactory factory, Guid userId, Guid categoryId)
    {
        _factory = factory;
        _userId = userId;
        _categoryId = categoryId;
    }

    public async Task<AnalyzerResult> AnalyzeAsync(OperationContext context)
    {
        var subRepo = _factory.CreateRepository<Subscription>();
        var planRepo = _factory.CreateRepository<Plan>();
        var listingRepo = _factory.CreateRepository<Listing>();
        var catRepo = _factory.CreateRepository<Category>();

        var category = await catRepo.GetByIdAsync(_categoryId, context.CancellationToken);
        if (category == null)
            return AnalyzerResult.Fail("category_not_found");

        // كل الاشتراكات النشطة لهذا المستخدم - مرتّبة من الأقدم
        var activeSubs = await subRepo.GetAllWithPredicateAsync(s =>
            s.UserId == _userId &&
            (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial));

        var candidates = activeSubs
            .Where(s => s.IsCurrentlyActive)
            .OrderBy(s => s.StartDate)
            .ToList();

        if (candidates.Count == 0)
            return AnalyzerResult.Fail("no_active_subscription");

        // ابحث عن أقدم اشتراك فيه رصيد ويسمح بالفئة
        foreach (var sub in candidates)
        {
            var plan = await planRepo.GetByIdAsync(sub.PlanId, context.CancellationToken);
            if (plan == null) continue;

            // فحص الفئة
            if (!string.IsNullOrWhiteSpace(plan.AllowedCategorySlugs))
            {
                var allowed = plan.AllowedCategorySlugs
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (!allowed.Contains(category.Slug, StringComparer.OrdinalIgnoreCase))
                    continue; // الفئة غير مسموح بها في هذه الباقة - جرّب التالية
            }

            // فحص الرصيد - عَدّ العروض المرتبطة بهذا الاشتراك تحديداً
            var consumed = await listingRepo.CountAsync(
                l => l.SubscriptionId == sub.Id && l.Status != ListingStatus.Closed,
                cancellationToken: context.CancellationToken);

            var remaining = plan.MaxListings == -1
                ? int.MaxValue
                : plan.MaxListings - consumed;

            if (remaining <= 0)
                continue; // لا رصيد - جرّب التالية

            // وجدنا اشتراكاً صالحاً - اربط
            context.Set("linked_subscription", sub);
            context.Set("linked_plan", plan);
            context.Set("linked_consumed_before", consumed);

            return new AnalyzerResult
            {
                Passed = true,
                Message = $"linked_to_{plan.Slug}",
                Data = new Dictionary<string, object>
                {
                    ["subscription_id"] = sub.Id,
                    ["plan_id"] = plan.Id,
                    ["plan_slug"] = plan.Slug,
                    ["consumed"] = consumed,
                    ["max"] = plan.MaxListings,
                    ["remaining_after"] = plan.MaxListings == -1 ? -1 : remaining - 1
                }
            };
        }

        return AnalyzerResult.Fail(
            $"no_subscription_with_quota_for_category: {category.Slug}");
    }
}

/// <summary>
/// محلل استهلاك الحصة (PostAnalyzer):
///
/// يعمل بعد تنفيذ العملية بنجاح. يقرأ من الـ context:
///   - linked_subscription: الاشتراك الذي تم ربطه
///   - listing: العرض المُنشأ
///
/// ثم:
///   - يحدّث listing.SubscriptionId/PlanIdSnapshot/BillingPeriod
///   - يزيد subscription.UsedListingsCount
///
/// النتيجة المحاسبية:
///   COUNT(listings WHERE SubscriptionId = sub.Id) == sub.UsedListingsCount
/// </summary>
public class QuotaConsumptionAnalyzer : IOperationAnalyzer
{
    private readonly IRepositoryFactory _factory;

    public string Name => "QuotaConsumptionAnalyzer";

    public IReadOnlyList<string> WatchedTagKeys => new[] { "subscription_check" };

    public QuotaConsumptionAnalyzer(IRepositoryFactory factory) => _factory = factory;

    public async Task<AnalyzerResult> AnalyzeAsync(OperationContext context)
    {
        if (!context.TryGet<Subscription>("linked_subscription", out var sub) || sub == null)
            return AnalyzerResult.Warning("no_linked_subscription");

        if (!context.TryGet<Listing>("listing", out var listing) || listing == null)
            return AnalyzerResult.Warning("no_listing_in_context");

        if (!context.TryGet<Plan>("linked_plan", out var plan) || plan == null)
            return AnalyzerResult.Warning("no_linked_plan");

        // ربط العرض بالاشتراك (لقطة)
        listing.SubscriptionId = sub.Id;
        listing.PlanIdSnapshot = plan.Id;
        listing.BillingPeriodStart = sub.StartDate;
        listing.BillingPeriodEnd = sub.EndDate;
        listing.OperationId = context.Operation.Id;

        var listingRepo = _factory.CreateRepository<Listing>();
        await listingRepo.UpdateAsync(listing, context.CancellationToken);

        // استهلاك حصة على الاشتراك
        sub.UsedListingsCount += 1;
        var subRepo = _factory.CreateRepository<Subscription>();
        await subRepo.UpdateAsync(sub, context.CancellationToken);

        return new AnalyzerResult
        {
            Passed = true,
            Message = $"consumed_1_from_{plan.Slug}",
            Data = new Dictionary<string, object>
            {
                ["subscription_id"] = sub.Id,
                ["used_count"] = sub.UsedListingsCount,
                ["max"] = plan.MaxListings
            }
        };
    }
}
