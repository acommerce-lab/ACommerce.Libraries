using ACommerce.Client.AppConfig.Services;
using ACommerce.Templates.Customer.Services;

namespace Ashare.Shared.Services;

/// <summary>
/// مساعد لحماية الصفحات الكاملة على مستوى الكود (مثلاً في OnInitializedAsync).
/// إذا كانت الميزة معطّلة يعيد توجيه المستخدم إلى المسار البديل المُمرَّر،
/// ويعيد <c>false</c> ليُنهي بقية المنطق في الصفحة بأمان.
/// </summary>
public sealed class FeatureGuard(IFeatureFlags features, IAppNavigationService navigation)
{
    /// <summary>
    /// تحقق من علامة. لو معطّلة: navigate لـ <paramref name="redirectTo"/> ثم return false.
    /// </summary>
    public bool RequireOrRedirect(string feature, string redirectTo = "/", bool fallback = true)
    {
        if (features.IsEnabled(feature, fallback))
        {
            return true;
        }

        navigation.NavigateTo(redirectTo);
        return false;
    }

    /// <summary>اختصار: <c>true</c> إذا الميزة مفعّلة.</summary>
    public bool IsEnabled(string feature, bool fallback = true) => features.IsEnabled(feature, fallback);
}
