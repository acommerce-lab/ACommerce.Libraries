namespace Ashare.Shared.Services;

/// <summary>
/// واجهة لخدمة دورة حياة التطبيق
/// تُستخدم لإعلام المكونات عند العودة للتطبيق أو مغادرته
/// </summary>
public interface IAppLifecycleService
{
    /// <summary>
    /// حدث يُطلق عند استئناف التطبيق (العودة من الخلفية)
    /// </summary>
    event Func<Task>? AppResumed;

    /// <summary>
    /// حدث يُطلق عند إيقاف التطبيق مؤقتاً (الذهاب للخلفية)
    /// </summary>
    event Func<Task>? AppPaused;

    /// <summary>
    /// حدث يُطلق عند تسجيل دخول المستخدم بنجاح
    /// يُستخدم لتسجيل Push Token مع الخادم
    /// </summary>
    event Func<Task>? UserLoggedIn;

    /// <summary>
    /// إطلاق حدث تسجيل الدخول
    /// </summary>
    Task NotifyUserLoggedInAsync();
}
