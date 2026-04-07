using ACommerce.OperationEngine.Core;

namespace Ashare.Api2.Services;

/// <summary>
/// محلل الاشتراك - يعمل كـ PreAnalyzer داخل القيد المحاسبي.
/// يفحص الاشتراك والباقة قبل تنفيذ العملية، ويفشل القيد إن لم يكن مسموحاً.
///
/// يُضاف للقيد عبر:
///   .Analyze(new SubscriptionAnalyzer(guard, userId, categoryId, SubscriptionCheckKind.CreateListing))
///
/// يراقب العلامات التي تحتوي subscription_check.
/// </summary>
public class SubscriptionAnalyzer : IOperationAnalyzer
{
    private readonly SubscriptionGuard _guard;
    private readonly Guid _userId;
    private readonly Guid? _categoryId;
    private readonly SubscriptionCheckKind _kind;

    public SubscriptionAnalyzer(
        SubscriptionGuard guard,
        Guid userId,
        SubscriptionCheckKind kind,
        Guid? categoryId = null)
    {
        _guard = guard;
        _userId = userId;
        _kind = kind;
        _categoryId = categoryId;
    }

    public string Name => $"SubscriptionAnalyzer({_kind})";

    /// <summary>
    /// نراقب أي قيد عليه علامة subscription_check.
    /// </summary>
    public IReadOnlyList<string> WatchedTagKeys => new[] { "subscription_check" };

    public async Task<AnalyzerResult> AnalyzeAsync(OperationContext context)
    {
        SubscriptionCheckResult check = _kind switch
        {
            SubscriptionCheckKind.CreateListing when _categoryId.HasValue
                => await _guard.CheckCanCreateListingAsync(_userId, _categoryId.Value, context.CancellationToken),

            SubscriptionCheckKind.FeatureListing
                => await _guard.CheckCanFeatureListingAsync(_userId, context.CancellationToken),

            SubscriptionCheckKind.SendMessage
                => await _guard.CheckCanSendMessageAsync(_userId, context.CancellationToken),

            _ => new SubscriptionCheckResult(false, "unknown_check_kind")
        };

        // ضع نتيجة الفحص في الـ context (للوصول لها لاحقاً في Execute)
        context.Set("subscription_check", check);

        if (!check.Allowed)
        {
            return new AnalyzerResult
            {
                Passed = false,
                Message = $"subscription_required: {check.Reason}",
                IsBlocking = true,
                Data = new Dictionary<string, object>
                {
                    ["reason"] = check.Reason ?? "denied",
                    ["plan_slug"] = check.Plan?.Slug ?? "none",
                    ["check_kind"] = _kind.ToString()
                }
            };
        }

        return new AnalyzerResult
        {
            Passed = true,
            Message = $"subscription_ok ({check.Plan?.Slug})",
            Data = new Dictionary<string, object>
            {
                ["plan_slug"] = check.Plan?.Slug ?? "none",
                ["remaining_listings"] = check.RemainingListings ?? -1
            }
        };
    }
}

public enum SubscriptionCheckKind
{
    CreateListing,
    FeatureListing,
    SendMessage
}
