using System.Text.Json;
using ACommerce.Client.AppConfig.Models;
using ACommerce.Client.Core.Storage;
using Microsoft.Extensions.Logging;

namespace ACommerce.Client.AppConfig.Services;

/// <summary>
/// مخزن AppConfig في طبقة المستخدم — يحتفظ بـ snapshot في الذاكرة وفي storage محلي.
/// يُعتبر مرجعاً للـ overrides القادمة من الخادم.
/// Layered resolution: Storage → Memory → Defaults (المسؤولية تنتقل للـ Consumer).
/// </summary>
public sealed class AppConfigStore(
    IStorageService storage,
    ILogger<AppConfigStore>? logger = null)
{
    private const string StorageKey = "acommerce.appconfig.snapshot.v1";
    private const string EtagKey = "acommerce.appconfig.etag.v1";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private AppConfigSnapshot? _current;
    private readonly object _lock = new();

    /// <summary>يطلق عند تحديث الـ snapshot — تستهلكه طبقات UI لإعادة بناء الواجهة.</summary>
    public event Action? SnapshotChanged;

    /// <summary>الـ snapshot الحالية (قد تكون null قبل التحميل الأول).</summary>
    public AppConfigSnapshot? Current
    {
        get { lock (_lock) return _current; }
    }

    /// <summary>تحميل آخر snapshot محفوظة من Storage عند بدء التطبيق.</summary>
    public async Task LoadFromStorageAsync()
    {
        try
        {
            var raw = await storage.GetAsync(StorageKey);
            if (string.IsNullOrWhiteSpace(raw)) return;

            var snap = JsonSerializer.Deserialize<AppConfigSnapshot>(raw, JsonOpts);
            if (snap != null)
            {
                lock (_lock) _current = snap;
                SnapshotChanged?.Invoke();
                logger?.LogDebug("AppConfig loaded from storage (version={Version})", snap.Version);
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to load AppConfig from storage");
        }
    }

    /// <summary>تحديث الـ snapshot الحالية وحفظها في Storage.</summary>
    public async Task SaveAsync(AppConfigSnapshot snapshot)
    {
        lock (_lock) _current = snapshot;

        try
        {
            var raw = JsonSerializer.Serialize(snapshot, JsonOpts);
            await storage.SetAsync(StorageKey, raw);
            await storage.SetAsync(EtagKey, snapshot.Version);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to persist AppConfig to storage");
        }

        SnapshotChanged?.Invoke();
    }

    /// <summary>ETag المحفوظ من آخر جلب ناجح — يُستخدم في If-None-Match.</summary>
    public async Task<string?> GetEtagAsync()
    {
        try { return await storage.GetAsync(EtagKey); }
        catch { return null; }
    }

    // ─── Lookups مع layered fallback ────────────────────────────

    public string GetString(string key, string fallback) =>
        Current?.Strings.TryGetValue(key, out var v) == true && !string.IsNullOrEmpty(v) ? v : fallback;

    public string? GetThemeToken(string key, string mode = "light") =>
        Current?.Theme.TryGetValue(mode, out var bucket) == true
            && bucket.TryGetValue(key, out var v) ? v : null;

    public bool IsFeatureEnabled(string key, bool fallback = false) =>
        Current?.Features.TryGetValue(key, out var v) == true ? v : fallback;

    public string GetConfig(string key, string fallback) =>
        Current?.Config.TryGetValue(key, out var v) == true && !string.IsNullOrEmpty(v) ? v : fallback;

    public int GetConfigInt(string key, int fallback) =>
        int.TryParse(GetConfig(key, fallback.ToString()), out var i) ? i : fallback;

    public bool GetConfigBool(string key, bool fallback) =>
        bool.TryParse(GetConfig(key, fallback.ToString()), out var b) ? b : fallback;
}
