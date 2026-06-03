using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ACommerce.AppConfig.Contracts;
using ACommerce.AppConfig.DTOs;
using ACommerce.AppConfig.Entities;
using ACommerce.AppConfig.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ACommerce.AppConfig.Services;

public class AppConfigService(
    DbContext db,
    IMemoryCache cache,
    IFeatureFlagsService featureFlags,
    ILogger<AppConfigService> logger) : IAppConfigService
{
    private const string StringsCacheKey = "AppConfig:Strings:All";
    private const string ThemeCacheKey = "AppConfig:Theme:All";
    private const string EntriesCacheKey = "AppConfig:Entries:All";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions HashJsonOptions = new()
    {
        WriteIndented = false
    };

    private DbSet<UiString> Strings => db.Set<UiString>();
    private DbSet<ThemeToken> Themes => db.Set<ThemeToken>();
    private DbSet<AppConfigEntry> Entries => db.Set<AppConfigEntry>();

    public async Task<AppConfigSnapshot> GetSnapshotAsync(
        string language, string? platform, string? appVersion, CancellationToken cancellationToken = default)
    {
        // Sequential — these four loaders share the same DbContext, which is not thread-safe.
        // Running them in parallel throws "A second operation was started on this context"
        // on the first request after cache invalidation. After the cache is warm there's no
        // DB hit, so the lost parallelism is essentially free.
        var strings = await GetActiveStringsAsync(language, cancellationToken);
        var theme   = await GetActiveThemeAsync(cancellationToken);
        var entries = await GetActiveEntriesAsync(cancellationToken);
        var flags   = await featureFlags.EvaluateAllAsync(platform, appVersion, cancellationToken);

        var snapshot = new AppConfigSnapshot
        {
            FetchedAt = DateTime.UtcNow,
            Language = language,
            Platform = platform,
            AppVersion = appVersion,
            Features = new Dictionary<string, bool>(flags),
            Strings = strings,
            Theme = theme,
            Config = entries
        };

        // Compute deterministic ETag (Version)
        snapshot.Version = ComputeHash(snapshot);
        return snapshot;
    }

    public void InvalidateCache()
    {
        cache.Remove(StringsCacheKey);
        cache.Remove(ThemeCacheKey);
        cache.Remove(EntriesCacheKey);
        // Note: FeatureFlagsService manages its own cache invalidation on writes.
        logger.LogDebug("AppConfig caches invalidated");
    }

    // ─── Snapshot builders ──────────────────────────────────────

    private async Task<Dictionary<string, string>> GetActiveStringsAsync(string language, CancellationToken ct)
    {
        var all = await GetAllStringsCachedAsync(ct);
        return all
            .Where(s => s.IsActive && s.Language == language)
            .ToDictionary(s => s.Key, s => s.Value);
    }

    private async Task<Dictionary<string, Dictionary<string, string>>> GetActiveThemeAsync(CancellationToken ct)
    {
        var all = await GetAllThemesCachedAsync(ct);
        var result = new Dictionary<string, Dictionary<string, string>>
        {
            ["light"] = new(),
            ["dark"] = new()
        };

        foreach (var token in all.Where(t => t.IsActive))
        {
            var bucket = token.Mode == ThemeMode.Dark ? "dark" : "light";
            result[bucket][token.Key] = token.Value;
        }

        return result;
    }

    private async Task<Dictionary<string, string>> GetActiveEntriesAsync(CancellationToken ct)
    {
        var all = await GetAllEntriesCachedAsync(ct);
        return all.Where(e => e.IsPublic).ToDictionary(e => e.Key, e => e.Value);
    }

    // ─── Cached fetches ─────────────────────────────────────────

    private async Task<IReadOnlyList<UiString>> GetAllStringsCachedAsync(CancellationToken ct)
        => (await cache.GetOrCreateAsync(StringsCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await Strings.AsNoTracking().Where(s => !s.IsDeleted).ToListAsync(ct);
        })) ?? [];

    private async Task<IReadOnlyList<ThemeToken>> GetAllThemesCachedAsync(CancellationToken ct)
        => (await cache.GetOrCreateAsync(ThemeCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await Themes.AsNoTracking().Where(t => !t.IsDeleted).ToListAsync(ct);
        })) ?? [];

    private async Task<IReadOnlyList<AppConfigEntry>> GetAllEntriesCachedAsync(CancellationToken ct)
        => (await cache.GetOrCreateAsync(EntriesCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await Entries.AsNoTracking().Where(e => !e.IsDeleted).ToListAsync(ct);
        })) ?? [];

    private static string ComputeHash(AppConfigSnapshot snapshot)
    {
        // Hash only the content (not the timestamp/version) so ETag is deterministic.
        var hashable = new
        {
            snapshot.Language,
            snapshot.Platform,
            snapshot.AppVersion,
            snapshot.Features,
            snapshot.Strings,
            snapshot.Theme,
            snapshot.Config
        };
        var json = JsonSerializer.Serialize(hashable, HashJsonOptions);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }

    // ═══════════════════════════════════════════════════════════
    // UiString CRUD
    // ═══════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<UiStringDto>> ListUiStringsAsync(string? language = null, CancellationToken ct = default)
    {
        var query = Strings.AsNoTracking().Where(s => !s.IsDeleted);
        if (!string.IsNullOrWhiteSpace(language))
            query = query.Where(s => s.Language == language);

        return await query
            .OrderBy(s => s.Key).ThenBy(s => s.Language)
            .Select(s => ToDto(s))
            .ToListAsync(ct);
    }

    public async Task<UiStringDto> UpsertUiStringAsync(UpsertUiStringDto dto, CancellationToken ct = default)
    {
        var existing = await Strings.FirstOrDefaultAsync(
            s => s.Key == dto.Key && s.Language == dto.Language && !s.IsDeleted, ct);

        if (existing != null)
        {
            existing.Value = dto.Value;
            existing.IsActive = dto.IsActive;
            existing.Note = dto.Note;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new UiString
            {
                Id = Guid.NewGuid(),
                Key = dto.Key,
                Language = dto.Language,
                Value = dto.Value,
                IsActive = dto.IsActive,
                Note = dto.Note,
                CreatedAt = DateTime.UtcNow
            };
            await Strings.AddAsync(existing, ct);
        }

        await db.SaveChangesAsync(ct);
        InvalidateCache();
        return ToDto(existing);
    }

    public async Task DeleteUiStringAsync(Guid id, CancellationToken ct = default)
    {
        var s = await Strings.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s == null) return;
        s.IsDeleted = true;
        s.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        InvalidateCache();
    }

    // ═══════════════════════════════════════════════════════════
    // ThemeToken CRUD
    // ═══════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<ThemeTokenDto>> ListThemeTokensAsync(CancellationToken ct = default)
    {
        return await Themes.AsNoTracking()
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Mode).ThenBy(t => t.Key)
            .Select(t => ToDto(t))
            .ToListAsync(ct);
    }

    public async Task<ThemeTokenDto> UpsertThemeTokenAsync(UpsertThemeTokenDto dto, CancellationToken ct = default)
    {
        var existing = await Themes.FirstOrDefaultAsync(
            t => t.Key == dto.Key && t.Mode == dto.Mode && !t.IsDeleted, ct);

        if (existing != null)
        {
            existing.Value = dto.Value;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new ThemeToken
            {
                Id = Guid.NewGuid(),
                Key = dto.Key,
                Mode = dto.Mode,
                Value = dto.Value,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            await Themes.AddAsync(existing, ct);
        }

        await db.SaveChangesAsync(ct);
        InvalidateCache();
        return ToDto(existing);
    }

    public async Task DeleteThemeTokenAsync(Guid id, CancellationToken ct = default)
    {
        var t = await Themes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t == null) return;
        t.IsDeleted = true;
        t.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        InvalidateCache();
    }

    // ═══════════════════════════════════════════════════════════
    // AppConfigEntry CRUD
    // ═══════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<AppConfigEntryDto>> ListAppConfigEntriesAsync(CancellationToken ct = default)
    {
        return await Entries.AsNoTracking()
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.Key)
            .Select(e => ToDto(e))
            .ToListAsync(ct);
    }

    public async Task<AppConfigEntryDto> UpsertAppConfigEntryAsync(UpsertAppConfigEntryDto dto, CancellationToken ct = default)
    {
        var existing = await Entries.FirstOrDefaultAsync(e => e.Key == dto.Key && !e.IsDeleted, ct);

        if (existing != null)
        {
            existing.Value = dto.Value;
            existing.ValueType = dto.ValueType;
            existing.IsPublic = dto.IsPublic;
            existing.Description = dto.Description;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new AppConfigEntry
            {
                Id = Guid.NewGuid(),
                Key = dto.Key,
                Value = dto.Value,
                ValueType = dto.ValueType,
                IsPublic = dto.IsPublic,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };
            await Entries.AddAsync(existing, ct);
        }

        await db.SaveChangesAsync(ct);
        InvalidateCache();
        return ToDto(existing);
    }

    public async Task DeleteAppConfigEntryAsync(Guid id, CancellationToken ct = default)
    {
        var e = await Entries.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null) return;
        e.IsDeleted = true;
        e.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        InvalidateCache();
    }

    // ─── DTO mappers ────────────────────────────────────────────

    private static UiStringDto ToDto(UiString s) => new()
    {
        Id = s.Id,
        Key = s.Key,
        Language = s.Language,
        Value = s.Value,
        IsActive = s.IsActive,
        Note = s.Note
    };

    private static ThemeTokenDto ToDto(ThemeToken t) => new()
    {
        Id = t.Id,
        Key = t.Key,
        Mode = t.Mode,
        Value = t.Value,
        IsActive = t.IsActive
    };

    private static AppConfigEntryDto ToDto(AppConfigEntry e) => new()
    {
        Id = e.Id,
        Key = e.Key,
        Value = e.Value,
        ValueType = e.ValueType,
        IsPublic = e.IsPublic,
        Description = e.Description
    };
}
