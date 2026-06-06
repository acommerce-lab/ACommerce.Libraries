using System.Net.Http.Json;

namespace Ashare.Admin.Services;

/// <summary>
/// خدمة إدارة AppConfig من اللوحة — أعلام الميزات + النصوص + ألوان الثيم + الإعدادات العامة.
/// تستدعي /api/admin/appconfig/* في Ashare API.
/// </summary>
public class AdminAppConfigService : AdminServiceBase
{
    public AdminAppConfigService(IConfiguration config, AdminAuthStateProvider auth) : base(config, auth) { }

    // ─── Feature Flags ──────────────────────────────────────────

    public async Task<List<FeatureFlagVm>> GetFeatureFlagsAsync()
    {
        await AuthAsync();
        try
        {
            return await Http.GetFromJsonAsync<List<FeatureFlagVm>>("/api/admin/appconfig/features")
                   ?? new();
        }
        catch { return new(); }
    }

    public async Task<bool> UpsertFeatureFlagAsync(FeatureFlagVm flag)
    {
        await AuthAsync();
        try
        {
            var resp = await Http.PostAsJsonAsync("/api/admin/appconfig/features", flag);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> SetFeatureEnabledAsync(FeatureFlagVm flag, bool enabled)
    {
        flag.Enabled = enabled;
        return await UpsertFeatureFlagAsync(flag);
    }

    public async Task<bool> DeleteFeatureFlagAsync(Guid id)
    {
        await AuthAsync();
        try { return (await Http.DeleteAsync($"/api/admin/appconfig/features/{id}")).IsSuccessStatusCode; }
        catch { return false; }
    }

    // ─── UI Strings ─────────────────────────────────────────────

    public async Task<List<UiStringVm>> GetStringsAsync(string? language = null)
    {
        await AuthAsync();
        var url = "/api/admin/appconfig/strings" + (string.IsNullOrEmpty(language) ? "" : $"?language={language}");
        try { return await Http.GetFromJsonAsync<List<UiStringVm>>(url) ?? new(); }
        catch { return new(); }
    }

    public async Task<bool> UpsertStringAsync(UiStringVm s)
    {
        await AuthAsync();
        try { return (await Http.PostAsJsonAsync("/api/admin/appconfig/strings", s)).IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<bool> DeleteStringAsync(Guid id)
    {
        await AuthAsync();
        try { return (await Http.DeleteAsync($"/api/admin/appconfig/strings/{id}")).IsSuccessStatusCode; }
        catch { return false; }
    }

    // ─── Theme Tokens ───────────────────────────────────────────

    public async Task<List<ThemeTokenVm>> GetThemeTokensAsync()
    {
        await AuthAsync();
        try { return await Http.GetFromJsonAsync<List<ThemeTokenVm>>("/api/admin/appconfig/theme") ?? new(); }
        catch { return new(); }
    }

    public async Task<bool> UpsertThemeTokenAsync(ThemeTokenVm t)
    {
        await AuthAsync();
        try { return (await Http.PostAsJsonAsync("/api/admin/appconfig/theme", t)).IsSuccessStatusCode; }
        catch { return false; }
    }

    // ─── Generic Config Entries ─────────────────────────────────

    public async Task<List<AppConfigEntryVm>> GetEntriesAsync()
    {
        await AuthAsync();
        try { return await Http.GetFromJsonAsync<List<AppConfigEntryVm>>("/api/admin/appconfig/entries") ?? new(); }
        catch { return new(); }
    }

    public async Task<bool> UpsertEntryAsync(AppConfigEntryVm e)
    {
        await AuthAsync();
        try { return (await Http.PostAsJsonAsync("/api/admin/appconfig/entries", e)).IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<bool> DeleteEntryAsync(Guid id)
    {
        await AuthAsync();
        try { return (await Http.DeleteAsync($"/api/admin/appconfig/entries/{id}")).IsSuccessStatusCode; }
        catch { return false; }
    }
}

// ════════════════════════════════════════════════════════════════
// View models — تطابق DTOs الخادمية (Upsert*Dto / *Dto)
// ════════════════════════════════════════════════════════════════

public class FeatureFlagVm
{
    public Guid Id { get; set; }
    public string Key { get; set; } = "";
    public bool Enabled { get; set; }
    public string? Platforms { get; set; }
    public string? MinAppVersion { get; set; }
    public string? MaxAppVersion { get; set; }
    public string? Description { get; set; }
    public bool RequiresClientRestart { get; set; }
}

public class UiStringVm
{
    public Guid Id { get; set; }
    public string Key { get; set; } = "";
    public string Language { get; set; } = "ar";
    public string Value { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}

public class ThemeTokenVm
{
    public Guid Id { get; set; }
    public string Key { get; set; } = "";
    public int Mode { get; set; } // 0 = Light, 1 = Dark
    public string Value { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public class AppConfigEntryVm
{
    public Guid Id { get; set; }
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public int ValueType { get; set; } // ConfigValueType enum index
    public bool IsPublic { get; set; } = true;
    public string? Description { get; set; }
}
