using ACommerce.Client.AppConfig.Models;
using ACommerce.Client.Core.Http;
using Microsoft.Extensions.Logging;

namespace ACommerce.Client.AppConfig.Services;

/// <summary>
/// HTTP client لجلب AppConfig snapshot من الخادم.
/// ملاحظة: الـCache الخادمي يجعل الجلب رخيصاً (cache-hit في الذاكرة)،
/// وحقل Version في الـsnapshot يسمح للعميل بالتعرف على عدم التغيير محلياً.
/// </summary>
public sealed class AppConfigClient(IApiClient api, ILogger<AppConfigClient>? logger = null)
{
    // الخدمة الافتراضية المسجَّلة في ServiceRegistry لتطبيقات Ashare
    private const string ServiceName = "Ashare";

    /// <summary>
    /// جلب snapshot كاملة من الخادم.
    /// إذا حصل خطأ شبكي يعود الـ Result بـ Error بدون رمي استثناء.
    /// </summary>
    public async Task<AppConfigSnapshotResult> GetSnapshotAsync(
        string language,
        string? platform = null,
        string? appVersion = null,
        string? lastKnownVersion = null,
        CancellationToken ct = default)
    {
        var query = $"?lang={Uri.EscapeDataString(language)}";
        if (!string.IsNullOrWhiteSpace(platform))   query += $"&platform={Uri.EscapeDataString(platform)}";
        if (!string.IsNullOrWhiteSpace(appVersion)) query += $"&version={Uri.EscapeDataString(appVersion)}";

        try
        {
            var snap = await api.GetAsync<AppConfigSnapshot>(
                ServiceName, $"/api/appconfig/snapshot{query}", ct);

            if (snap == null)
                return AppConfigSnapshotResult.Failed(new InvalidOperationException("Empty AppConfig response"));

            // معالجة محلية لـ "not modified": إذا الـVersion نفسها لا داعي للحفظ
            if (!string.IsNullOrWhiteSpace(lastKnownVersion)
                && string.Equals(lastKnownVersion, snap.Version, StringComparison.OrdinalIgnoreCase))
            {
                return AppConfigSnapshotResult.NotModified();
            }

            return AppConfigSnapshotResult.Fresh(snap);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "AppConfig snapshot fetch failed (will use cache/defaults)");
            return AppConfigSnapshotResult.Failed(ex);
        }
    }
}

public sealed class AppConfigSnapshotResult
{
    public AppConfigSnapshot? Snapshot { get; init; }
    public bool IsNotModified { get; init; }
    public Exception? Error { get; init; }

    public bool HasFreshSnapshot => Snapshot != null && !IsNotModified && Error == null;

    public static AppConfigSnapshotResult Fresh(AppConfigSnapshot s) => new() { Snapshot = s };
    public static AppConfigSnapshotResult NotModified() => new() { IsNotModified = true };
    public static AppConfigSnapshotResult Failed(Exception e) => new() { Error = e };
}
