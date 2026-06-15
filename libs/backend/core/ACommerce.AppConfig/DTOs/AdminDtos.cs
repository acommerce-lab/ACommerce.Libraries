using ACommerce.AppConfig.Enums;

namespace ACommerce.AppConfig.DTOs;

// ─── AppConfigEntry ──────────────────────────────────────────────

public class AppConfigEntryDto
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public ConfigValueType ValueType { get; set; }
    public bool IsPublic { get; set; }
    public string? Description { get; set; }
}

public class UpsertAppConfigEntryDto
{
    public required string Key { get; set; }
    public required string Value { get; set; }
    public ConfigValueType ValueType { get; set; } = ConfigValueType.String;
    public bool IsPublic { get; set; } = true;
    public string? Description { get; set; }
}

// ─── UiString ────────────────────────────────────────────────────

public class UiStringDto
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Language { get; set; }
    public required string Value { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
}

public class UpsertUiStringDto
{
    public required string Key { get; set; }
    public required string Language { get; set; }
    public required string Value { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}

// ─── ThemeToken ──────────────────────────────────────────────────

public class ThemeTokenDto
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public ThemeMode Mode { get; set; }
    public required string Value { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertThemeTokenDto
{
    public required string Key { get; set; }
    public ThemeMode Mode { get; set; } = ThemeMode.Light;
    public required string Value { get; set; }
    public bool IsActive { get; set; } = true;
}

// ─── FeatureFlag ─────────────────────────────────────────────────

public class FeatureFlagDto
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public bool Enabled { get; set; }
    public string? Platforms { get; set; }
    public string? MinAppVersion { get; set; }
    public string? MaxAppVersion { get; set; }
    public string? Description { get; set; }
    public bool RequiresClientRestart { get; set; }
}

public class UpsertFeatureFlagDto
{
    public required string Key { get; set; }
    public bool Enabled { get; set; }
    public string? Platforms { get; set; }
    public string? MinAppVersion { get; set; }
    public string? MaxAppVersion { get; set; }
    public string? Description { get; set; }
    public bool RequiresClientRestart { get; set; }
}
