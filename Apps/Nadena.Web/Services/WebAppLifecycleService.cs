using Nadena.Shared.Services;

namespace Nadena.Web.Services;

/// <summary>
/// Web implementation of the app lifecycle service.
/// The browser has no native resume/pause, so only the login event is used
/// (e.g. to register a push token / refresh data after sign-in).
/// </summary>
public class WebAppLifecycleService : IAppLifecycleService
{
    public event Func<Task>? AppResumed;
    public event Func<Task>? AppPaused;
    public event Func<Task>? UserLoggedIn;

    public async Task NotifyUserLoggedInAsync()
    {
        if (UserLoggedIn == null) return;

        foreach (var handler in UserLoggedIn.GetInvocationList().Cast<Func<Task>>())
        {
            try
            {
                await handler();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebAppLifecycle] handler error: {ex.Message}");
            }
        }
    }
}
