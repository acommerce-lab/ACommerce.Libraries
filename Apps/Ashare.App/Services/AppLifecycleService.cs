using Ashare.Shared.Services;

namespace Ashare.App.Services;

/// <summary>
/// خدمة لإدارة أحداث دورة حياة التطبيق (MAUI Lifecycle)
/// تُستخدم لإعلام المكونات عند العودة للتطبيق
/// </summary>
public class AppLifecycleService : IAppLifecycleService
{
    /// <summary>
    /// حدث يُطلق عند استئناف التطبيق (العودة من الخلفية)
    /// </summary>
    public event Func<Task>? AppResumed;

    /// <summary>
    /// حدث يُطلق عند إيقاف التطبيق مؤقتاً (الذهاب للخلفية)
    /// </summary>
    public event Func<Task>? AppPaused;

    /// <summary>
    /// إعلام جميع المستمعين بأن التطبيق استؤنف
    /// </summary>
    public async Task NotifyResumedAsync()
    {
        Console.WriteLine("[AppLifecycle] 📱 App RESUMED - notifying listeners");
        if (AppResumed != null)
        {
            foreach (var handler in AppResumed.GetInvocationList().Cast<Func<Task>>())
            {
                try
                {
                    await handler();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AppLifecycle] ⚠️ Handler error: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// إعلام جميع المستمعين بأن التطبيق توقف مؤقتاً
    /// </summary>
    public async Task NotifyPausedAsync()
    {
        Console.WriteLine("[AppLifecycle] 💤 App PAUSED");
        if (AppPaused != null)
        {
            foreach (var handler in AppPaused.GetInvocationList().Cast<Func<Task>>())
            {
                try
                {
                    await handler();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AppLifecycle] ⚠️ Handler error: {ex.Message}");
                }
            }
        }
    }
}
