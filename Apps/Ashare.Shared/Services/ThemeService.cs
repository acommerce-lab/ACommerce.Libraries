using ACommerce.Templates.Customer.Services;

namespace Ashare.Shared.Services;

/// <summary>
/// خدمة إدارة المظهر (الوضع الفاتح/الداكن)
/// </summary>
public class ThemeService
{
    private bool _isDarkMode;
    private const string ThemeKey = "app_theme";
    private readonly IStorageService _storageService;

    public ThemeService(IStorageService storageService)
    {
        _storageService = storageService;
        _ = LoadThemeAsync();
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                _ = SaveThemeAsync();
                OnThemeChanged?.Invoke();
            }
        }
    }

    public string ThemeClass => IsDarkMode ? "ac-dark" : "ac-light";

    public event Action? OnThemeChanged;

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
    }

    public void SetDarkMode(bool isDark)
    {
        IsDarkMode = isDark;
    }

    private async Task LoadThemeAsync()
    {
        try
        {
            var saved = await _storageService.GetAsync(ThemeKey);
            if (!string.IsNullOrEmpty(saved))
            {
                _isDarkMode = saved == "dark";
            }
        }
        catch
        {
            // Ignore errors
        }
    }

    private async Task SaveThemeAsync()
    {
        try
        {
            await _storageService.SetAsync(ThemeKey, _isDarkMode ? "dark" : "light");
        }
        catch
        {
            // Ignore errors
        }
    }
}
