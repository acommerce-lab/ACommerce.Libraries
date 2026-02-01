using ACommerce.Client.Core.Storage;

namespace ACommerce.Templates.Customer.Services;

/// <summary>
/// Service for managing app settings like theme, language, notifications
/// </summary>
public class AppSettingsService
{
    private readonly IStorageService _storage;
    private const string SettingsKey = "app_settings";

    public AppSettings Settings { get; private set; } = new();
    public bool IsInitialized { get; private set; }

    public event Action? OnSettingsChanged;

    public AppSettingsService(IStorageService storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        try
        {
            var saved = await _storage.GetAsync<AppSettings>(SettingsKey);
            if (saved != null)
            {
                Settings = saved;
            }
        }
        catch
        {
            Settings = new AppSettings();
        }
        finally
        {
            IsInitialized = true;
            NotifySettingsChanged();
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        Settings.Theme = theme;
        await SaveSettingsAsync();
    }

    public async Task SetLanguageAsync(string language)
    {
        Settings.Language = language;
        await SaveSettingsAsync();
    }

    public async Task SetNotificationsEnabledAsync(bool enabled)
    {
        Settings.NotificationsEnabled = enabled;
        await SaveSettingsAsync();
    }

    public async Task SetNotificationSoundAsync(bool enabled)
    {
        Settings.NotificationSound = enabled;
        await SaveSettingsAsync();
    }

    public async Task SetNotificationVibrationAsync(bool enabled)
    {
        Settings.NotificationVibration = enabled;
        await SaveSettingsAsync();
    }

    public async Task UpdateSettingsAsync(Action<AppSettings> update)
    {
        update(Settings);
        await SaveSettingsAsync();
    }

    private async Task SaveSettingsAsync()
    {
        await _storage.SetAsync(SettingsKey, Settings);
        NotifySettingsChanged();
    }

    private void NotifySettingsChanged() => OnSettingsChanged?.Invoke();
}

/// <summary>
/// App settings model stored in local storage
/// </summary>
public class AppSettings
{
    // Appearance
    public string Theme { get; set; } = AppThemes.System;
    public string Language { get; set; } = AppLanguages.Arabic;

    // Notifications
    public bool NotificationsEnabled { get; set; } = true;
    public bool NotificationSound { get; set; } = true;
    public bool NotificationVibration { get; set; } = true;

    // Preferences
    public string? DefaultLocationId { get; set; }
    public string? DefaultPaymentMethodId { get; set; }
    public bool SaveOrderHistory { get; set; } = true;
    public bool RememberLastSearch { get; set; } = true;

    // Privacy
    public bool ShareAnalytics { get; set; } = true;
    public bool ShareCrashReports { get; set; } = true;
}

public static class AppThemes
{
    public const string Light = "light";
    public const string Dark = "dark";
    public const string System = "system";

    public static string GetLabel(string theme) => theme switch
    {
        Light => "فاتح",
        Dark => "داكن",
        System => "تلقائي (حسب النظام)",
        _ => theme
    };

    public static string GetIcon(string theme) => theme switch
    {
        Light => "bi-sun",
        Dark => "bi-moon",
        System => "bi-circle-half",
        _ => "bi-palette"
    };
}

public static class AppLanguages
{
    public const string Arabic = "ar";
    public const string English = "en";

    public static string GetLabel(string language) => language switch
    {
        Arabic => "العربية",
        English => "English",
        _ => language
    };

    public static string GetNativeLabel(string language) => language switch
    {
        Arabic => "العربية",
        English => "English",
        _ => language
    };
}
