using Microsoft.Extensions.DependencyInjection;

namespace Ashare.Shared.Services;

/// <summary>
/// خدمة إدارة وضع الضيف مع حفظ الحالة في التخزين
/// </summary>
public class GuestModeService
{
    private const string GuestModeKey = "ashare_guest_mode";
    private readonly IServiceScopeFactory _scopeFactory;
    
    private static bool _isGuestMode;
    private static bool _isInitialized;
    private static readonly object _lock = new();

    public GuestModeService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// هل المستخدم في وضع الضيف
    /// </summary>
    public bool IsGuestMode => _isGuestMode;

    /// <summary>
    /// تحميل حالة الضيف من التخزين (يجب استدعاءها عند بدء التطبيق)
    /// </summary>
    public async Task InitializeAsync()
    {
        lock (_lock)
        {
            if (_isInitialized) return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var storage = scope.ServiceProvider.GetService<IStorageService>();
            if (storage != null)
            {
                var value = await storage.GetAsync(GuestModeKey);
                lock (_lock)
                {
                    _isGuestMode = value == "true";
                    _isInitialized = true;
                }
                Console.WriteLine($"[GuestModeService] Initialized from storage: {_isGuestMode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GuestModeService] Initialize error: {ex.Message}");
        }
    }

    /// <summary>
    /// تفعيل وضع الضيف
    /// </summary>
    public async Task EnableGuestModeAsync()
    {
        lock (_lock)
        {
            _isGuestMode = true;
            _isInitialized = true;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var storage = scope.ServiceProvider.GetService<IStorageService>();
            if (storage != null)
            {
                await storage.SetAsync(GuestModeKey, "true");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GuestModeService] EnableGuestMode error: {ex.Message}");
        }

        OnGuestModeChanged?.Invoke();
        Console.WriteLine("[GuestModeService] Guest mode enabled");
    }

    /// <summary>
    /// تفعيل وضع الضيف (نسخة متزامنة للتوافق)
    /// </summary>
    public void EnableGuestMode()
    {
        _ = EnableGuestModeAsync();
    }

    /// <summary>
    /// إلغاء وضع الضيف
    /// </summary>
    public async Task DisableGuestModeAsync()
    {
        lock (_lock)
        {
            _isGuestMode = false;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var storage = scope.ServiceProvider.GetService<IStorageService>();
            if (storage != null)
            {
                await storage.RemoveAsync(GuestModeKey);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GuestModeService] DisableGuestMode error: {ex.Message}");
        }

        OnGuestModeChanged?.Invoke();
        Console.WriteLine("[GuestModeService] Guest mode disabled");
    }

    /// <summary>
    /// إلغاء وضع الضيف (نسخة متزامنة للتوافق)
    /// </summary>
    public void DisableGuestMode()
    {
        _ = DisableGuestModeAsync();
    }

    /// <summary>
    /// حدث تغيير وضع الضيف
    /// </summary>
    public event Action? OnGuestModeChanged;
}
