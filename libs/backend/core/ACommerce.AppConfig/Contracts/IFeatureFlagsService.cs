using ACommerce.AppConfig.DTOs;

namespace ACommerce.AppConfig.Contracts;

/// <summary>
/// خدمة استعلام / تعديل علامات الميزات.
/// </summary>
public interface IFeatureFlagsService
{
    /// <summary>
    /// تقييم ميزة لعميل بمواصفات معينة (Platform + Version).
    /// يرجع false إذا لم تكن الميزة موجودة أو معطّلة أو خارج الإصدار/المنصة.
    /// </summary>
    Task<bool> IsEnabledAsync(
        string key,
        string? platform = null,
        string? appVersion = null,
        CancellationToken ct = default);

    /// <summary>
    /// تقييم كل العلامات لعميل بمواصفات معينة — يستخدمه الـ snapshot builder.
    /// </summary>
    Task<IReadOnlyDictionary<string, bool>> EvaluateAllAsync(
        string? platform = null,
        string? appVersion = null,
        CancellationToken ct = default);

    // ─── CRUD ───────────────────────────────────────────────────

    Task<IReadOnlyList<FeatureFlagDto>> ListAsync(CancellationToken ct = default);
    Task<FeatureFlagDto?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task<FeatureFlagDto> UpsertAsync(UpsertFeatureFlagDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
