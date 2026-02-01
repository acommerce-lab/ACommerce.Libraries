using Microsoft.JSInterop;
using System.Text.Json;

namespace Order.Shared.Services;

/// <summary>
/// خدمة التخزين المحلي للتطبيق
/// </summary>
public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Dictionary<string, object?> _cache = new();

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (_cache.TryGetValue(key, out var cached))
            return cached is T value ? value : default;

        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrEmpty(json)) return default;

            var result = JsonSerializer.Deserialize<T>(json);
            _cache[key] = result;
            return result;
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
            _cache[key] = value;
        }
        catch
        {
            // Ignore storage errors
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            _cache.Remove(key);
        }
        catch
        {
            // Ignore
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
            _cache.Clear();
        }
        catch
        {
            // Ignore
        }
    }
}

/// <summary>
/// خدمة إدارة حالة المصادقة
/// </summary>
public class AuthStateService
{
    private readonly LocalStorageService _storage;
    private const string TokenKey = "auth_token";
    private const string UserKey = "current_user";

    public event Action? OnAuthStateChanged;

    public AuthStateService(LocalStorageService storage)
    {
        _storage = storage;
    }

    public bool IsAuthenticated { get; private set; }
    public UserData? CurrentUser { get; private set; }
    public string? Token { get; private set; }

    public async Task InitializeAsync()
    {
        Token = await _storage.GetAsync<string>(TokenKey);
        CurrentUser = await _storage.GetAsync<UserData>(UserKey);
        IsAuthenticated = !string.IsNullOrEmpty(Token) && CurrentUser != null;
    }

    public async Task LoginAsync(string token, UserData user)
    {
        Token = token;
        CurrentUser = user;
        IsAuthenticated = true;

        await _storage.SetAsync(TokenKey, token);
        await _storage.SetAsync(UserKey, user);

        OnAuthStateChanged?.Invoke();
    }

    public async Task UpdateUserAsync(UserData user)
    {
        CurrentUser = user;
        await _storage.SetAsync(UserKey, user);
        OnAuthStateChanged?.Invoke();
    }

    public async Task LogoutAsync()
    {
        Token = null;
        CurrentUser = null;
        IsAuthenticated = false;

        await _storage.RemoveAsync(TokenKey);
        await _storage.RemoveAsync(UserKey);

        OnAuthStateChanged?.Invoke();
    }
}

/// <summary>
/// بيانات المستخدم
/// </summary>
public class UserData
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public bool IsNewUser { get; set; }
}

/// <summary>
/// خدمة إعدادات التطبيق
/// </summary>
public class AppSettingsService
{
    private readonly LocalStorageService _storage;
    private const string SettingsKey = "app_settings";

    public event Action? OnSettingsChanged;

    public AppSettingsService(LocalStorageService storage)
    {
        _storage = storage;
    }

    public AppSettings Settings { get; private set; } = new();

    public async Task InitializeAsync()
    {
        var settings = await _storage.GetAsync<AppSettings>(SettingsKey);
        Settings = settings ?? new AppSettings();
    }

    public async Task UpdateSettingsAsync(AppSettings settings)
    {
        Settings = settings;
        await _storage.SetAsync(SettingsKey, settings);
        OnSettingsChanged?.Invoke();
    }

    public async Task SetThemeAsync(string theme)
    {
        Settings.Theme = theme;
        await _storage.SetAsync(SettingsKey, Settings);
        OnSettingsChanged?.Invoke();
    }

    public async Task SetLanguageAsync(string language)
    {
        Settings.Language = language;
        await _storage.SetAsync(SettingsKey, Settings);
        OnSettingsChanged?.Invoke();
    }

    public async Task SetNotificationsEnabledAsync(bool enabled)
    {
        Settings.NotificationsEnabled = enabled;
        await _storage.SetAsync(SettingsKey, Settings);
        OnSettingsChanged?.Invoke();
    }
}

/// <summary>
/// إعدادات التطبيق
/// </summary>
public class AppSettings
{
    public string Theme { get; set; } = "light"; // light, dark, system
    public string Language { get; set; } = "ar"; // ar, en
    public bool NotificationsEnabled { get; set; } = true;
    public bool SoundEnabled { get; set; } = true;
    public bool VibrationEnabled { get; set; } = true;
    public SavedLocation? DefaultLocation { get; set; }
    public SavedCarInfo? DefaultCarInfo { get; set; }
}

/// <summary>
/// موقع محفوظ
/// </summary>
public class SavedLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Description { get; set; }
    public string? Name { get; set; }
}

/// <summary>
/// معلومات السيارة المحفوظة
/// </summary>
public class SavedCarInfo
{
    public string? CarModel { get; set; }
    public string? CarColor { get; set; }
    public string? PlateNumber { get; set; }
}
