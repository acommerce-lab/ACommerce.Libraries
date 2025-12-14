using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Templates.Customer.Services;

/// <summary>
/// Guest mode management service with state persistence
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

    /// <summary>
    /// Is user in guest mode
    /// </summary>
    public bool IsGuestMode => _isGuestMode;

    /// <summary>
    /// Load guest state from storage (should be called at app startup)
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
    /// Enable guest mode
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
    /// Enable guest mode (sync version for compatibility)
    /// </summary>
    public void EnableGuestMode()
    {
        _ = EnableGuestModeAsync();
    }

    /// <summary>
    /// Disable guest mode
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
    /// Disable guest mode (sync version for compatibility)
    /// </summary>
    public void DisableGuestMode()
    {
        _ = DisableGuestModeAsync();
    }

    /// <summary>
    /// Guest mode changed event
    /// </summary>
    public event Action? OnGuestModeChanged;
}
