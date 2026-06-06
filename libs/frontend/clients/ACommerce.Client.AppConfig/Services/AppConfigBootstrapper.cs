using Microsoft.Extensions.Logging;

namespace ACommerce.Client.AppConfig.Services;

/// <summary>
/// تنسيق دورة حياة AppConfig في تطبيق العميل:
///   1) عند الإقلاع: تحميل من Storage (instant) ثم تحديث من الخادم في الخلفية
///   2) دورياً: تحديث كل RefreshInterval إن كان التطبيق مفتوحاً
///   3) عند الاستئناف من الخلفية (AppResumed): تحديث فوري
/// </summary>
public sealed class AppConfigBootstrapper(
    AppConfigClient client,
    AppConfigStore store,
    AppConfigClientOptions options,
    ILogger<AppConfigBootstrapper>? logger = null) : IDisposable
{
    private Timer? _timer;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// يُستدعى مرة واحدة عند بدء التطبيق. لا يرمي — أي فشل يسجَّل فقط.
    /// </summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // 1. Load cached snapshot — UI يبدأ فوراً
        await store.LoadFromStorageAsync();

        // 2. Refresh in background
        _ = RefreshAsync(_cts.Token);

        // 3. Schedule periodic refresh
        if (options.RefreshInterval > TimeSpan.Zero)
        {
            _timer = new Timer(_ => _ = RefreshAsync(_cts.Token),
                null, options.RefreshInterval, options.RefreshInterval);
        }
    }

    /// <summary>تحديث يدوي (مثلاً عند AppResumed من MAUI).</summary>
    public async Task RefreshAsync(CancellationToken ct = default)
    {
        try
        {
            var lastVersion = store.Current?.Version ?? await store.GetEtagAsync();
            var result = await client.GetSnapshotAsync(
                options.Language, options.Platform, options.AppVersion, lastVersion, ct);

            if (result.HasFreshSnapshot)
            {
                await store.SaveAsync(result.Snapshot!);
                logger?.LogInformation("AppConfig refreshed (version={Version})", result.Snapshot!.Version);
            }
            else if (result.IsNotModified)
            {
                logger?.LogDebug("AppConfig unchanged");
            }
            else if (result.Error != null)
            {
                logger?.LogWarning(result.Error, "AppConfig refresh failed; using cached");
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "AppConfig refresh threw");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _cts?.Cancel();
        _cts?.Dispose();
    }
}

public sealed class AppConfigClientOptions
{
    /// <summary>اللغة الحالية ("ar", "en", "ur").</summary>
    public string Language { get; set; } = "ar";

    /// <summary>المنصة الحالية ("android", "ios", "web"). يستخدمها الخادم لتقييم Feature Flags.</summary>
    public string? Platform { get; set; }

    /// <summary>إصدار التطبيق ("1.16"). يستخدمه الخادم لتقييم Min/Max version.</summary>
    public string? AppVersion { get; set; }

    /// <summary>فترة التحديث الدورية. الافتراضي دقيقة لاستجابة سريعة لتغييرات الأدمن.</summary>
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(1);
}
