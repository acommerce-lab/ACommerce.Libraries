using ACommerce.AppConfig.DTOs;

namespace ACommerce.AppConfig.Contracts;

/// <summary>
/// خدمة جلب وتعديل إعدادات التطبيق (نصوص، ثيم، إعدادات عامة).
/// </summary>
public interface IAppConfigService
{
    /// <summary>
    /// بناء لقطة كاملة من الإعدادات للعميل، مفلترة على اللغة/المنصة/الإصدار.
    /// النتيجة قابلة للتخزين المؤقت داخلياً، تُحدَّث عند أي تعديل.
    /// </summary>
    Task<AppConfigSnapshot> GetSnapshotAsync(
        string language,
        string? platform,
        string? appVersion,
        CancellationToken cancellationToken = default);

    /// <summary>إبطال الـ cache المحلي بعد أي تعديل من خدمة إدارية.</summary>
    void InvalidateCache();

    // ─── UiString CRUD ──────────────────────────────────────────

    Task<IReadOnlyList<UiStringDto>> ListUiStringsAsync(string? language = null, CancellationToken ct = default);
    Task<UiStringDto> UpsertUiStringAsync(UpsertUiStringDto dto, CancellationToken ct = default);
    Task DeleteUiStringAsync(Guid id, CancellationToken ct = default);

    // ─── ThemeToken CRUD ────────────────────────────────────────

    Task<IReadOnlyList<ThemeTokenDto>> ListThemeTokensAsync(CancellationToken ct = default);
    Task<ThemeTokenDto> UpsertThemeTokenAsync(UpsertThemeTokenDto dto, CancellationToken ct = default);
    Task DeleteThemeTokenAsync(Guid id, CancellationToken ct = default);

    // ─── AppConfigEntry CRUD ────────────────────────────────────

    Task<IReadOnlyList<AppConfigEntryDto>> ListAppConfigEntriesAsync(CancellationToken ct = default);
    Task<AppConfigEntryDto> UpsertAppConfigEntryAsync(UpsertAppConfigEntryDto dto, CancellationToken ct = default);
    Task DeleteAppConfigEntryAsync(Guid id, CancellationToken ct = default);
}
