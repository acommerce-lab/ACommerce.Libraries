using Ashare.App.Services;

namespace Ashare.App;

public partial class App : Application
{
    private readonly AppLifecycleService _lifecycleService;
    private readonly IPushNotificationService _pushService;

    public App(AppLifecycleService lifecycleService, IPushNotificationService pushService)
    {
        InitializeComponent();
        _lifecycleService = lifecycleService;
        _pushService = pushService;

        // الاشتراك في حدث تسجيل الدخول لتسجيل Push Token
        _lifecycleService.UserLoggedIn += OnUserLoggedIn;
    }

    private async Task OnUserLoggedIn()
    {
        Console.WriteLine("[App] 🔐 User logged in - refreshing push token registration");
        await _pushService.RefreshTokenRegistrationAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new MainPage()) { Title = "عشير - Ashare" };

        // الاستماع لأحداث دورة حياة النافذة
        window.Resumed += OnWindowResumed;
        window.Stopped += OnWindowStopped;
        window.Activated += OnWindowActivated;

        return window;
    }

    /// <summary>
    /// يُستدعى عند استئناف النافذة (العودة من الخلفية)
    /// </summary>
    private async void OnWindowResumed(object? sender, EventArgs e)
    {
        Console.WriteLine("[App] 📱 Window RESUMED event fired");
        await _lifecycleService.NotifyResumedAsync();
    }

    /// <summary>
    /// يُستدعى عند تفعيل النافذة (التركيز عليها)
    /// </summary>
    private async void OnWindowActivated(object? sender, EventArgs e)
    {
        Console.WriteLine("[App] 🔆 Window ACTIVATED event fired");
        // نرسل حدث الاستئناف أيضاً عند التفعيل لضمان التقاط العودة
        await _lifecycleService.NotifyResumedAsync();
    }

    /// <summary>
    /// يُستدعى عند إيقاف النافذة (الذهاب للخلفية)
    /// </summary>
    private async void OnWindowStopped(object? sender, EventArgs e)
    {
        Console.WriteLine("[App] 💤 Window STOPPED event fired");
        await _lifecycleService.NotifyPausedAsync();
    }
}
