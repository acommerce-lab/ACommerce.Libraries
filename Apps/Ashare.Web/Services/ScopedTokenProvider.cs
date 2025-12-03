using ACommerce.Client.Core.Interceptors;
using Ashare.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ashare.Web.Services;

/// <summary>
/// Wrapper لـ TokenManager يمكن استخدامه كـ singleton
/// يستخدم IServiceScopeFactory للوصول إلى TokenManager الـ scoped
/// مع cache ثابت لتجنب إعادة التهيئة في كل طلب
/// </summary>
public class ScopedTokenProvider : ITokenProvider
{
    private readonly IServiceScopeFactory _scopeFactory;

    // Static cache - shared across all instances and scopes
    private static string? _cachedToken;
    private static DateTime? _cachedExpiresAt;
    private static bool _isInitialized;
    private static readonly object _lock = new();
    private static bool _eventSubscribed;

    public ScopedTokenProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        // Subscribe to token change events (only once)
        lock (_lock)
        {
            if (!_eventSubscribed)
            {
                TokenStorageService.OnTokenChanged += OnTokenChanged;
                _eventSubscribed = true;
                Console.WriteLine("[ScopedTokenProvider] Subscribed to TokenStorageService.OnTokenChanged");
            }
        }
    }

    private static void OnTokenChanged(string? token, DateTime? expiresAt)
    {
        UpdateCache(token, expiresAt);
    }

    public async Task<string?> GetTokenAsync()
    {
        // First check static cache for performance
        lock (_lock)
        {
            if (_isInitialized)
            {
                if (!string.IsNullOrEmpty(_cachedToken))
                {
                    // Check if expired
                    if (_cachedExpiresAt.HasValue && DateTime.UtcNow >= _cachedExpiresAt.Value)
                    {
                        Console.WriteLine("[ScopedTokenProvider] Cached token expired");
                        _cachedToken = null;
                        _cachedExpiresAt = null;
                        _isInitialized = false; // Allow retry
                    }
                    else
                    {
                        Console.WriteLine("[ScopedTokenProvider] Returning cached token");
                        return _cachedToken;
                    }
                }
                // If _isInitialized but _cachedToken is null, it means user logged out
                // Return null without trying storage
                else
                {
                    Console.WriteLine("[ScopedTokenProvider] No token in cache (logged out)");
                    return null;
                }
            }
        }

        // Not initialized, try to load from storage via TokenManager
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var tokenManager = scope.ServiceProvider.GetRequiredService<ACommerce.Client.Auth.TokenManager>();
            var token = await tokenManager.GetTokenAsync();

            // Only cache if we got a real token
            // If null (e.g., during prerendering), don't cache - try again next time
            if (!string.IsNullOrEmpty(token))
            {
                lock (_lock)
                {
                    _cachedToken = token;
                    _isInitialized = true;
                    Console.WriteLine("[ScopedTokenProvider] Token loaded from storage and cached");
                }
            }
            else
            {
                Console.WriteLine("[ScopedTokenProvider] No token from storage (might be prerendering)");
            }

            return token;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScopedTokenProvider] Error getting token: {ex.Message}");
            return _cachedToken; // Return whatever we have cached
        }
    }

    /// <summary>
    /// Update the static cache when token changes
    /// Called by TokenManager when token is set
    /// </summary>
    public static void UpdateCache(string? token, DateTime? expiresAt)
    {
        lock (_lock)
        {
            _cachedToken = token;
            _cachedExpiresAt = expiresAt;
            _isInitialized = true;
            Console.WriteLine($"[ScopedTokenProvider] Cache updated: {(token != null ? "token set" : "token cleared")}");
        }
    }

    /// <summary>
    /// Clear the static cache on logout
    /// </summary>
    public static void ClearCache()
    {
        lock (_lock)
        {
            _cachedToken = null;
            _cachedExpiresAt = null;
            _isInitialized = false;
            Console.WriteLine("[ScopedTokenProvider] Cache cleared");
        }
    }
}
