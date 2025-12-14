using Microsoft.Extensions.DependencyInjection;

namespace Ashare.Shared.Services;

/// <summary>
/// Guest mode management service with state persistence
/// Re-exports ACommerce.Templates.Customer.Services.GuestModeService for namespace compatibility
/// </summary>
public class GuestModeService
{
    private const string GuestModeKey = "acommerce_guest_mode";
    private readonly IServiceScopeFactory _scopeFactory;
    
    private static bool _isGuestMode;
    private static bool _isInitialized;
    private static readonly object _lock = new();

    public GuestModeService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public bool IsGuestMode => _isGuestMode;

    public async Task InitializeAsync()
    {
        lock (_lock)
        {
            if (_isInitialized) return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var storage = scope.ServiceProvider.GetService<ACommerce.Templates.Customer.Services.IStorageService>();
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
            var storage = scope.ServiceProvider.GetService<ACommerce.Templates.Customer.Services.IStorageService>();
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

    public void EnableGuestMode()
    {
        _ = EnableGuestModeAsync();
    }

    public async Task DisableGuestModeAsync()
    {
        lock (_lock)
        {
            _isGuestMode = false;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var storage = scope.ServiceProvider.GetService<ACommerce.Templates.Customer.Services.IStorageService>();
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

    public void DisableGuestMode()
    {
        _ = DisableGuestModeAsync();
    }

    public event Action? OnGuestModeChanged;
}
