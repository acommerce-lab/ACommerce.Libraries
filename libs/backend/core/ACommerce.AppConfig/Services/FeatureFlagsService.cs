using ACommerce.AppConfig.Contracts;
using ACommerce.AppConfig.DTOs;
using ACommerce.AppConfig.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ACommerce.AppConfig.Services;

public class FeatureFlagsService(
    DbContext db,
    IMemoryCache cache,
    ILogger<FeatureFlagsService> logger) : IFeatureFlagsService
{
    private const string AllFlagsCacheKey = "AppConfig:Flags:All";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private DbSet<FeatureFlag> Flags => db.Set<FeatureFlag>();

    public async Task<bool> IsEnabledAsync(
        string key, string? platform = null, string? appVersion = null, CancellationToken ct = default)
    {
        var all = await GetAllCachedAsync(ct);
        var flag = all.FirstOrDefault(f => f.Key == key);
        return flag != null && Evaluate(flag, platform, appVersion);
    }

    public async Task<IReadOnlyDictionary<string, bool>> EvaluateAllAsync(
        string? platform = null, string? appVersion = null, CancellationToken ct = default)
    {
        var all = await GetAllCachedAsync(ct);
        return all.ToDictionary(f => f.Key, f => Evaluate(f, platform, appVersion));
    }

    public async Task<IReadOnlyList<FeatureFlagDto>> ListAsync(CancellationToken ct = default)
    {
        var all = await GetAllCachedAsync(ct);
        return all.Select(ToDto).ToList();
    }

    public async Task<FeatureFlagDto?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        var all = await GetAllCachedAsync(ct);
        var f = all.FirstOrDefault(x => x.Key == key);
        return f == null ? null : ToDto(f);
    }

    public async Task<FeatureFlagDto> UpsertAsync(UpsertFeatureFlagDto dto, CancellationToken ct = default)
    {
        var existing = await Flags.FirstOrDefaultAsync(f => f.Key == dto.Key && !f.IsDeleted, ct);
        if (existing != null)
        {
            existing.Enabled = dto.Enabled;
            existing.Platforms = NormalizePlatforms(dto.Platforms);
            existing.MinAppVersion = dto.MinAppVersion;
            existing.MaxAppVersion = dto.MaxAppVersion;
            existing.Description = dto.Description;
            existing.RequiresClientRestart = dto.RequiresClientRestart;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new FeatureFlag
            {
                Id = Guid.NewGuid(),
                Key = dto.Key,
                Enabled = dto.Enabled,
                Platforms = NormalizePlatforms(dto.Platforms),
                MinAppVersion = dto.MinAppVersion,
                MaxAppVersion = dto.MaxAppVersion,
                Description = dto.Description,
                RequiresClientRestart = dto.RequiresClientRestart,
                CreatedAt = DateTime.UtcNow
            };
            await Flags.AddAsync(existing, ct);
        }

        await db.SaveChangesAsync(ct);
        cache.Remove(AllFlagsCacheKey);
        logger.LogInformation("FeatureFlag upserted: {Key} = {Enabled}", dto.Key, dto.Enabled);
        return ToDto(existing);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var f = await Flags.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (f == null) return;

        f.IsDeleted = true;
        f.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        cache.Remove(AllFlagsCacheKey);
    }

    // ─── Helpers ────────────────────────────────────────────────

    private async Task<IReadOnlyList<FeatureFlag>> GetAllCachedAsync(CancellationToken ct)
    {
        return (await cache.GetOrCreateAsync(AllFlagsCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await Flags.AsNoTracking().Where(f => !f.IsDeleted).ToListAsync(ct);
        })) ?? [];
    }

    private static bool Evaluate(FeatureFlag flag, string? platform, string? appVersion)
    {
        if (!flag.Enabled) return false;

        // Platform check (CSV: "android,ios" — empty/null = all)
        if (!string.IsNullOrWhiteSpace(flag.Platforms) && !string.IsNullOrWhiteSpace(platform))
        {
            var allowed = flag.Platforms
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(p => p.ToLowerInvariant());
            if (!allowed.Contains(platform.ToLowerInvariant())) return false;
        }

        // Version range check
        if (!string.IsNullOrWhiteSpace(appVersion) &&
            SemanticVersion.TryParse(appVersion, out var clientVer))
        {
            if (SemanticVersion.TryParse(flag.MinAppVersion, out var min) && clientVer < min)
                return false;
            if (SemanticVersion.TryParse(flag.MaxAppVersion, out var max) && clientVer > max)
                return false;
        }
        else if (!string.IsNullOrWhiteSpace(flag.MinAppVersion) || !string.IsNullOrWhiteSpace(flag.MaxAppVersion))
        {
            // العميل لم يُبلِّغ بإصداره لكن العلامة محدودة بإصدارات — نتعامل تحفظياً ونمنع.
            return false;
        }

        return true;
    }

    private static string? NormalizePlatforms(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var parts = input
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => p.ToLowerInvariant())
            .Distinct()
            .ToArray();
        return parts.Length == 0 ? null : string.Join(",", parts);
    }

    private static FeatureFlagDto ToDto(FeatureFlag f) => new()
    {
        Id = f.Id,
        Key = f.Key,
        Enabled = f.Enabled,
        Platforms = f.Platforms,
        MinAppVersion = f.MinAppVersion,
        MaxAppVersion = f.MaxAppVersion,
        Description = f.Description,
        RequiresClientRestart = f.RequiresClientRestart
    };
}
